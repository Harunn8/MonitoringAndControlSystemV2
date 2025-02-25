using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McsApplication.Models;
using McsCore.Entities;

namespace McsApplication.Responses
{
    public class TcpDeviceResponse
    {
        public string DeviceName { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public List<string> TcpFormat { get; set; }
        public List<TcpData> TcpData { get; set; }
    }
}
