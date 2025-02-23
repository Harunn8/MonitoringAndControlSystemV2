using System;
using Serilog;
using MQTTnet.Client.Options;
using EventBusMqtt.Connection.Base;
using MQTTnet;
using MQTTnet.Client;

namespace EventBusMqtt.Connection
{
    public class MqttConnection : IMqttConnection
    {
        private readonly IMqttClientOptions _mqttOptions;
        private readonly IMqttClient _mqttClient;
        private bool _disposed;
        private bool _isConnect = false;

        public MqttConnection(IMqttClientOptions mqttClientOptions, IMqttClient mqttClient)
        {
            _mqttOptions = mqttClientOptions;
            _mqttClient = mqttClient;

            if (!_mqttClient.IsConnected && !_disposed)
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
                    Log.Information("Disconnected from MQTT Server. Reconnecting...");
                    while (_mqttClient.IsConnected == false)
                    {
                        _mqttClient.ReconnectAsync();
                        Log.Error("Could not connect to MQTT Server");
                    }
                    _mqttClient.ConnectAsync(_mqttOptions).Wait();
                    Log.Information("Mqtt connection was establish");
                    _isConnect = true;
                });
            }
            catch (Exception ex) 
            {
                Log.Error("Failed to established MQTT connection: {ErrorMessage}", ex.Message);
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
