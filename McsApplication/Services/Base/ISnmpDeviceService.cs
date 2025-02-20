using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McsApplication.Models;
using McsApplication.Responses;
using McsCore.Entities;

namespace McsApplication.Services.Base
{
    public interface ISnmpDeviceService
    {
        public Task<List<SnmpDeviceResponse>> GetSnmpDeviceAsync();
        public Task<List<SnmpDeviceResponse>> GetSnmpDeviceById(string id);
        public Task AddSnmpDevice(SnmpDevice snmpDevice);
        public Task UpdateDevice(string id,SnmpDevice updatedSnmpDevice);
        public Task DeleteDevice(string id);
        public Task<SnmpDeviceModel> GetSnmpDeviceByIpAndPort(string ipAddress, int port);
    }
}
