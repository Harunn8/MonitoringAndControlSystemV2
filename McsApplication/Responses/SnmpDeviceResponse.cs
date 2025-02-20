using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McsApplication.Models;
using McsCore.Entities;

namespace McsApplication.Responses
{
    public class SnmpDeviceResponse
    {
        public string IpAddress { get; set; }
        public int port { get; set; }
        public string DeviceName { get; set; }
        public List<OidMapping> OidList { get; set; }
    }
}
