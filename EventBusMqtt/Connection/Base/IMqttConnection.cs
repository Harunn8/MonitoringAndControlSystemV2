using System;
using MQTTnet;
using MQTTnet.Client;

namespace EventBusMqtt.Connection.Base
{
    public interface IMqttConnection : IDisposable
    {
        void TryConnect();
        IMqttClient GetMqttClient();
    }
}
