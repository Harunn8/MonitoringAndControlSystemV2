using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using EventBusMqtt.Producer;
using McsApplication.Models;
using McsApplication.Responses;
using McsApplication.Services.Base;
using McsCore.Entities;
using MongoDB.Driver;
using MQTTnet.Client;
using Services.Base;
using MQTTnet.Protocol;
using Serilog;

namespace Services
{
    public class TcpService : ITcpDeviceService
    {
        private readonly IMongoCollection<TcpDevice> _tcpDevice;
        private readonly IMapper _mapper;
        private readonly MqttProducer _mqttProducer;
        private readonly CancellationTokenSource cancellationToken;
        private readonly ILogger _logger;
        private bool _isRunning = false;
        public TcpService(IMongoDatabase database, MqttProducer mqttProducer, IMapper mapper, ILogger logger)
        {
            _tcpDevice = database.GetCollection<TcpDevice>("TcpDevice");
            cancellationToken = new CancellationTokenSource();
            _mqttProducer = mqttProducer;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task AddTcpDevice(TcpDevice tcpDevice)
        {
            await _tcpDevice.InsertOneAsync(tcpDevice);
        }

        public async Task DeleteTcpDevice(string id)
        {
            await _tcpDevice.DeleteOneAsync(d => d.Id == id);
        }

        public async Task<List<TcpDeviceResponse>> GetTcpDeviceAsync()
        {
            var entityDevice = await _tcpDevice.Find(d => true).ToListAsync();

            var responseDevice = _mapper.Map<List<TcpDeviceResponse>>(entityDevice);

            return responseDevice;
        }

        public async Task<TcpDeviceResponse> GetTcpDeviceById(string id)
        {
            var entityDevice = await _tcpDevice.Find(d => d.Id == id).FirstOrDefaultAsync();

            var responseDevice = _mapper.Map<TcpDeviceResponse>(entityDevice);

            return responseDevice;
        }

        public async Task<TcpDeviceModel> GetTcpDeviceByIpAndPort(string ipAddress, int port)
        {
            var device = await _tcpDevice.Find(d => d.IpAddress == ipAddress && d.Port == port).FirstOrDefaultAsync();

            var modelDevice = _mapper.Map<TcpDeviceModel>(device);

            return modelDevice;
        }

        public static Dictionary<string, string> ParseTcpData(string rawData, List<TcpData> tcpDataList)
        {
            if (string.IsNullOrWhiteSpace(rawData) || tcpDataList == null || tcpDataList.Count == 0)
                return new Dictionary<string, string>();

            string[] parsedValues = rawData.Split(',').Select(s => s.Trim()).ToArray();

            Dictionary<string, string> result = new Dictionary<string, string>();

            for (int i = 0; i < parsedValues.Length && i < tcpDataList.Count; i++)
            {
                string parameterName = tcpDataList[i].ParameterName;
                string value = parsedValues[i];

                if (!string.IsNullOrEmpty(parameterName))
                {
                    result[parameterName] = value;
                }
            }

            return result;
        }

        public async Task StartCommunication(string ipAddress, int port, string tcpFormat, Action<Dictionary<string, string>> onDataReceived, CancellationToken cancellationToken)
        {
            try
            {
                var device = await GetTcpDeviceByIpAndPort(ipAddress, port);
                _logger.Information($"Connecting {device.DeviceName}");
                _mqttProducer.PublishMessage("telemetry/tcp",$"Connecting {device.DeviceName}", MqttQualityOfServiceLevel.AtLeastOnce);

                while (!cancellationToken.IsCancellationRequested && !_isRunning)
                {
                    using var client = new TcpClient();
                    await client.ConnectAsync(device.IpAddress, device.Port);
                    
                    _logger.Information($"Connected {device.DeviceName}");
                    _mqttProducer.PublishMessage("telemetry / tcp",$"Connected { device.DeviceName} ", MqttQualityOfServiceLevel.AtLeastOnce);

                    _isRunning = true;

                    using var stream = client.GetStream();

                    var message = Encoding.UTF8.GetBytes(tcpFormat);

                    await stream.WriteAsync(message, 0, message.Length, cancellationToken);

                    var buffer = new byte[1024];
                    var byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (byteCount > 0)
                    {
                        var data = Encoding.UTF8.GetString(buffer,0, byteCount);

                        Dictionary<string, string> parsedData = ParseTcpData(data, device.TcpData);
                        _mqttProducer.PublishMessage("telemetry/tcp", $"Parsed Data: {string.Join(", ", parsedData.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}", MqttQualityOfServiceLevel.AtMostOnce);

                        onDataReceived?.Invoke(parsedData);
                    }
                    client.Close();
                    await Task.Delay(200);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Error during TCP communication: {e.Message}");
                _mqttProducer.PublishMessage("telemetry/tcp", $"Error during TCP communication: {e.Message}", MqttQualityOfServiceLevel.AtMostOnce);
            }
        }

        public async Task StopCommunication(TcpClient client)
        {
            try
            {
                _isRunning = false;
                cancellationToken?.Cancel();
                cancellationToken?.Dispose();

                if (client != null && client.Connected)
                {
                    client.Close();
                    _logger.Information("Communication Stopped");
                    _mqttProducer.PublishMessage("telemetry/tcp", $"Communication Stopped", MqttQualityOfServiceLevel.AtMostOnce);
                }

                client = null;
            }
            catch (Exception e)
            {
                _logger.Error($"Error Stopping Communication: {e.Message}");
            }
        }

        public async Task UpdateTcpDevice(string id, TcpDevice tcpDevice)
        {
            await _tcpDevice.ReplaceOneAsync(d => d.Id == id, tcpDevice);
            _logger.Information($"Updated tcp device: {tcpDevice.DeviceName},{DateTime.Now}");
        }
    }
}
