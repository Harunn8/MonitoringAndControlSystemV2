﻿using System;
using SnmpSharpNet;
using EventBusMqtt.Producer;
using Services.Base;
using Microsoft.Extensions.Logging;
using MQTTnet.Protocol;
using Serilog;

namespace Services
{
    public class SnmpService : ISnmpService
    {
        private readonly MqttProducer _mqttProducer;
        private bool _isRunning = false;

        public SnmpService(MqttProducer mqttProducer)
        {
            _mqttProducer = mqttProducer;
        }

        public async Task StartSnmpCommunication(string ipAddress, int port, List<string> oidList, Action<string> onMessageReceived, CancellationToken cancellationToken)
        {
           _isRunning = true;

            UdpTarget target = new UdpTarget(new System.Net.IPAddress(System.Net.IPAddress.Parse(ipAddress).GetAddressBytes()), port, 1000, 1);

            while (_isRunning && cancellationToken.IsCancellationRequested) 
            {
                try
                {
                    Pdu pdu = new Pdu(PduType.Get); 
                    foreach(string oid in oidList)
                    {
                        pdu.VbList.Add(oid);
                    }

                    AgentParameters agentParameters = new AgentParameters(new OctetString("public"))
                    {
                        Version = SnmpVersion.Ver2
                    };

                    SnmpV2Packet response = (SnmpV2Packet)target.Request(pdu, agentParameters);

                    if(response != null && response.Pdu.ErrorStatus == 0) 
                    {
                        foreach(Vb vb in response.Pdu.VbList)
                        {
                            onMessageReceived?.Invoke($"OID{vb.Oid},{vb.Value}");
                            _mqttProducer.PublishMessage("telemetry/snmp", $"{vb.Oid},{vb.Value}", MqttQualityOfServiceLevel.AtMostOnce);
                            Console.WriteLine($"{vb.Oid},{vb.Value}");
                        }
                    }
                    else
                    {
                        onMessageReceived?.Invoke("SNMP result returned null or error status. Verify OIDs or IP address");
                        Log.Warning("SNMP result returned null or error status. Verify OIDs or IP address");
                    }
                }
                catch (Exception ex) 
                {
                    onMessageReceived?.Invoke($"Error occurred during SNMP query: {ex.Message}");
                    Log.Error($"Error occurred during SNMP query: {ex}");
                }

                await Task.Delay(200);
            }
            target.Close();
        }

        public void StopSnmpCommunication()
        {
            if(!_isRunning)
            {
                return;
            }
            _isRunning = false;
        }

        public async Task SendSnmpCommand(string ipAddress, int port, string oid, string value)
        {
            try
            {
                UdpTarget target = new UdpTarget(new System.Net.IPAddress(System.Net.IPAddress.Parse(ipAddress).GetAddressBytes()), port, 1000, 1);

                Pdu pdu = new Pdu(PduType.Set);
                pdu.VbList.Add(new Vb(new Oid(oid), new OctetString(value)));

                AgentParameters agentParameters = new AgentParameters(new OctetString("private"))
                {
                    Version = SnmpVersion.Ver2
                };

                SnmpV2Packet response = (SnmpV2Packet)target.Request(pdu, agentParameters);

                if (response != null && response.Pdu.ErrorStatus == 0)
                {
                    _mqttProducer.PublishMessage("telemetry", $"{oid},{value} command send was successfully", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
                    Log.Information($"Command was send successfully to {oid},{value}");
                }

                else
                {
                    _mqttProducer.PublishMessage("telemetry", $"Error! This command can not sended be successfully", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
                    Log.Information("Error! This command can not sended be successfully");
                }
            }
            catch(Exception ex) 
            {
                Log.Information("Error :", ex.Message);
            }
        }
    }
}
