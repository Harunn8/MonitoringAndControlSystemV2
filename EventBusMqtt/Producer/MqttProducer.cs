using System;
using EventBusMqtt.Connection;
using MQTTnet.Protocol;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using EventBusMqtt.Connection.Base;

namespace EventBusMqtt.Producer
{
    public class MqttProducer
    {
        private readonly IMqttConnection _mqttConnection;
        private readonly IMqttClient _mqttClient;
        private readonly IMqttClientOptions _mqttOptions;

        public MqttProducer(IMqttConnection mqttConnection,IMqttClient mqttClient, IMqttClientOptions mqttClientOptions)
        {
            _mqttConnection = mqttConnection;
            _mqttClient = mqttClient;
            _mqttOptions = mqttClientOptions;
        }

        public virtual bool GetMqttConnectionStatus()
        {
            return _mqttConnection.GetMqttClient().IsConnected;
        }

        public virtual void PublishMessage(string topic, string message, MqttQualityOfServiceLevel qosLevel)
        {
            var client = _mqttConnection.GetMqttClient();
            {
                var sendingMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel(qosLevel)
                    .WithPayload(message)
                    .Build();

                client.PublishAsync(sendingMessage).Wait();
            }
        }
    }
}