using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using MongoDB.Driver;
using McsApplication.Models;
using McsApplication.Responses;
using EventBusMqtt.Producer;
using McsCore.Entities;
using Serilog;
using MongoDB.Driver;
using MQTTnet.Protocol;

namespace Services.Base
{
    public interface ITcpDeviceService
    {
        public Task<List<TcpDeviceResponse>> GetTcpDeviceAsync();
        public Task<TcpDeviceResponse> GetTcpDeviceById(string id);
        public Task<TcpDeviceModel> GetTcpDeviceByIpAndPort(string ipAddress, int port);
        public Task AddTcpDevice(TcpDevice tcpDevice);
        public Task UpdateTcpDevice(string id, TcpDevice tcpDevice);
        public Task DeleteTcpDevice(string id);

        public Task StartCommunication(string ipAddress, int port, string tcpFormat, Action<Dictionary<string, string>> onDataReceived, CancellationToken cancellationToken);

        public Task StopCommunication(TcpClient client);
    }
}
