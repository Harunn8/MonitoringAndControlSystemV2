using System;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using EventBusMqtt.Producer;
using Services;
using Serilog.Core;
using McsCore.Entities;

namespace McsAPI.Controllers
{
    public class WebSocketHandlerTcp
    {
        private readonly TcpService _tcpService;
        private readonly MqttProducer _mqttProducer;
        private readonly Logger _logger;
        private CancellationTokenSource _cancellationTokenSource;

        public WebSocketHandlerTcp(TcpService tcpService, MqttProducer mqttProducer, Logger logger,CancellationTokenSource cancellationTokenSource)
        {
            _tcpService = tcpService;
            _mqttProducer = mqttProducer;
            _logger = logger;
            _cancellationTokenSource = cancellationTokenSource;

        }

        public async Task HandleAsyncTcp(HttpContext context, WebSocket webSocket)
        {
            if (webSocket == null)
            {
                throw new ArgumentException(nameof(webSocket));
            }

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            try
            {
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var command = JsonConvert.DeserializeObject<WebSocketCommand>(message);

                    switch (command.Action.ToLower())
                    {
                        case "starttcp":
                            _cancellationTokenSource = new CancellationTokenSource();
                            await StartTcpCommunication(command.Parameters, webSocket, _cancellationTokenSource.Token);
                            break;

                        case "stoptcp":
                            StopTcpCommunication();
                            await SendMessage(webSocket, "TCP Communication Stopped");
                            break;

                        default:
                            await SendMessage(webSocket, $"Unknown Command: {command.Action}");
                            break;
                    }
                }
                while (!result.CloseStatus.HasValue);

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in WebSocket Communication: {e.Message}");
            }
        }

        private async Task StartTcpCommunication(Dictionary<string, string> parameters, WebSocket webSocket, CancellationToken cancellationToken)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            if (!parameters.TryGetValue("ipAddress", out var ipAddress))
            {
                await SendMessage(webSocket, "IP Address is missing");
                return;
            }

            if (!parameters.TryGetValue("port", out var port))
            {
                await SendMessage(webSocket, "Port is missing");
                return;
            }

            var device = await _tcpService.GetTcpDeviceByIpAndPort(ipAddress, Convert.ToInt32(port));
            if (device == null)
            {
                await SendMessage(webSocket, "Device not found");
                return;
            }

            try
            {
                string tcpFormat = device.TcpFormat != null ? string.Join(",", device.TcpFormat) : string.Empty;

                await _tcpService.StartCommunication(device.IpAddress, device.Port, tcpFormat, async (parsedData) =>
                {
                    //await _deviceDataService.AddDeviceData(device.Id, "TCP", JsonConvert.SerializeObject(parsedData));

                    if (webSocket.State == WebSocketState.Open)
                    {
                        await SendMessage(webSocket, JsonConvert.SerializeObject(new { Device = device.DeviceName, Data = parsedData }));
                    }
                }, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error starting TCP communication: {e.Message}");
            }
        }

        private void StopTcpCommunication()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private async Task SendMessage(WebSocket webSocket, string message)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
