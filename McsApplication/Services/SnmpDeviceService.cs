using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McsApplication.Models;
using McsApplication.Responses;
using McsApplication.Services.Base;
using McsCore.Entities;
using MongoDB.Driver;
using AutoMapper;

namespace Services
{
    public class SnmpDeviceService : ISnmpDeviceService
    {
        private readonly IMongoCollection<SnmpDevice> _snmpDevice;
        private readonly IMapper _mapper;

        public SnmpDeviceService(IMongoDatabase database,IMapper mapper)
        {
            _snmpDevice = database.GetCollection<SnmpDevice>("SnmpDevice");
            _mapper = mapper;
        }

        public async Task AddSnmpDevice(SnmpDevice snmpDevice)
        {
            await _snmpDevice.InsertOneAsync(snmpDevice);
        }

        public async Task DeleteDevice(string id)
        {
            await _snmpDevice.DeleteOneAsync(d => d.Id == id);
        }

        public async Task<List<SnmpDeviceResponse>> GetSnmpDeviceAsync()
        {
            var entities = await _snmpDevice.Find(device => true).ToListAsync();

            var responses = _mapper.Map<List<SnmpDeviceResponse>>(entities);

            return responses;
        }

        public async Task<List<SnmpDeviceResponse>> GetSnmpDeviceById(string id)
        {
            var entities = await _snmpDevice.Find(d => d.Id == id).ToListAsync();

            var responses = _mapper.Map<List<SnmpDeviceResponse>>(entities);

            return responses;

        }

        public async Task<SnmpDeviceModel> GetSnmpDeviceByIpAndPort(string ipAddress, int port)
        {
            var device = await _snmpDevice.Find(d => d.IpAddress == ipAddress && d.Port == port).ToListAsync();

            var deviceModel = _mapper.Map<SnmpDeviceModel>(device);

            return deviceModel;
        }

        public async Task UpdateDevice(string id, SnmpDevice updatedSnmpDevice)
        {
            await _snmpDevice.ReplaceOneAsync(d => d.Id == id, updatedSnmpDevice);
        }
    }
}
