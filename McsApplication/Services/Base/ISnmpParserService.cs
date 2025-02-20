using McsCore.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Base
{
    public interface ISnmpParserService
    {
        public DeviceData Parse(string data, string deviceId);
    }
}
