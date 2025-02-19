using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using MQTTnet.Client.Options;
using EventBusMqtt.Connection.Base;
using MQTTnet;
using MQTTnet.Client;


namespace EventBusMqtt.Connection
{
    public class MqttConnection : IMqttConnection
    {
        IMqttClientOptions _mqttClientOptions;
        private IMqttClient _mqttClient;
        private ILogger _logger;
        private bool _disposed;
        private bool _isConnect = false;

        public MqttConnection(IMqttClientOptions mqttClientOptions, IMqttClient mqttClient, ILogger logger)
        {
            _mqttClient = mqttClient;
            _mqttClientOptions = mqttClientOptions;
            _logger = logger;

            if(!_mqttClient.IsConnected && !_disposed)
            {
                TryConnect();
            }
        }

        public void Dispose()
        {
            if(_disposed)
            {
                return;
            }
            try
            {
                _mqttClient.Dispose();
            }
            catch(Exception ex) 
            {
                throw;
            }
        }

        public void TryConnect()
        {
           try
            {
                _mqttClient.UseDisconnectedHandler(e =>
                {
                    _logger.Information("Disconnected from MQTT Server. Reconnecting...");
                    while (_mqttClient.IsConnected == false)
                    {
                        _mqttClient.ReconnectAsync();
                        _logger.Error("Could not connect to MQTT Server");
                    }
                    _mqttClient.ConnectAsync(_mqttClientOptions).Wait();
                    _logger.Information("Mqtt connection was establish");
                    _isConnect = true;
                });
            }
            catch (Exception ex) 
            {
                _logger.Error("Failed to established MQTT connection: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public IMqttClient GetMqttClient()
        {
            if(!_mqttClient.IsConnected && !_disposed)
            {
                TryConnect();
            }
            return _mqttClient;
        }
    }
}
