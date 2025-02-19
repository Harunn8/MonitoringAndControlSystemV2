using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McsApplication.Models;

namespace McsApplication.Responses
{
    public class DeviceDataResponse
    {
        public string DeviceId { get; set; }
        public string Oid { get; set; }
        public string Value { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
