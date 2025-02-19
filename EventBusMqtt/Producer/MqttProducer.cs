using System;
using EventBusMqtt.Connection;
using MQTTnet.Protocol;
using MQTTnet;
using MQTTnet.Client;

namespace EventBusMqtt.Producer
{
    public class MqttProducer
    {
        private readonly MqttConnection _mqttConnection;

        public MqttProducer(MqttConnection mqttConnection)
        {
            _mqttConnection = mqttConnection;
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