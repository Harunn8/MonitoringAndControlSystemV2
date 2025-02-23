using System;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using Services;
using EventBusMqtt.Producer;
using Serilog;
using Serilog.Core;
using McsCore.Entities;

namespace McsAPI.Controllers
{
    public class WebSocketHandlerSnmp
    {
        private readonly SnmpService _snmpService;
        private readonly SnmpDeviceService _snmpDeviceService;
        private MqttProducer _mqttProducer;
        private CancellationTokenSource _cancellationTokenSource;

        public WebSocketHandlerSnmp(SnmpService snmpService, SnmpDeviceService snmpDeviceService, MqttProducer mqttProducer, CancellationTokenSource cancellationTokenSource)
        {
            _snmpService = snmpService;
            _snmpDeviceService = snmpDeviceService;
            _mqttProducer = mqttProducer;
            _cancellationTokenSource = cancellationTokenSource;
        }

        public async Task HandleAsyncSnmp(HttpContext httpContext, WebSocket webSocket)
        {
            if(webSocket == null)
            {
                throw new ArgumentNullException(nameof(webSocket),"Web Socket can not be null");
            }

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            try
            {
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if(result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if(string.IsNullOrWhiteSpace(message))
                    {
                        await SendMessage(webSocket, "Empty or invalid message received");
                        continue;
                    }

                    var command = JsonConvert.DeserializeObject<WebSocketCommand>(message);
                    if(command == null || string.IsNullOrEmpty(command.Action))
                    {
                        await SendMessage(webSocket, "Invalid command format");
                        continue;
                    }

                    switch(command.Action.ToLower()) 
                    {
                        case "startcommunication":
                            _cancellationTokenSource = new CancellationTokenSource();
                            StartCommunication(command.Parameters, webSocket, _cancellationTokenSource.Token);
                            _mqttProducer.PublishMessage("telemetry/snmp", $"Communication Started",MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
                            break;
                        case "stopcommunication":
                            StopCommunication();
                            await SendMessage(webSocket, "Communication Stopped");
                            _mqttProducer.PublishMessage("telemetry/snmp", $"Communication Stopped", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
                            break;
                        default:
                            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                            break;
                    }
                }
                while (!result.CloseStatus.HasValue);
            }
            catch (Exception ex) 
            {
                Log.Error($"Error in WebSocket communication: {ex.Message}");
                await SendMessage(webSocket, $"An error occurred: {ex.Message}");
            }


            void StartCommunication(Dictionary<string, string> parameters, WebSocket webSocket, CancellationToken cancellationToken)
            {
                if (parameters == null || !parameters.TryGetValue("ipAddress", out var ipAddress))
                {
                    SendMessage(webSocket, "IP address is missing").Wait();
                    return;
                }

                if (!parameters.TryGetValue("port", out var portString) || !int.TryParse(portString, out var port))
                {
                    SendMessage(webSocket, "Port is missing").Wait();
                    return;
                }

                var result = _snmpDeviceService.GetSnmpDeviceByIpAndPort(ipAddress, port).Result;
                if (result == null)
                {
                    return;
                }


                var oidList = result.OidList;

                try
                {
                    var oidListAsString = oidList.Select(mapping => mapping.Oid).ToList();

                    _ = _snmpService.StartSnmpCommunication(ipAddress, result.Port, oidListAsString, async (data) =>
                    {
                        if (webSocket.State == WebSocketState.Open)
                        {
                            var jsonMessage = JsonConvert.SerializeObject(new
                            {

                            });
                            await SendMessage(webSocket, data);
                        }
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error starting communication: {ex.Message}");
                    SendMessage(webSocket, $"Error starting communication: {ex.Message}");
                }
            }
             void StopCommunication()
            {
                try
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;

                    _snmpService.StopSnmpCommunication();
                }
                catch (Exception ex)
                {
                    Log.Error($"Error stopping communication: {ex.Message}");
                }
            }

             async Task SendMessage(WebSocket webSocket, string message)
            {
                if (webSocket.State != WebSocketState.Open)
                {
                    Log.Warning("WebSocket is not open. Unable to send message.");
                    return;
                }

                var buffer = Encoding.UTF8.GetBytes(message);
                try
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error sending message: {ex.Message}");
                }
            }
        }
    }
}
