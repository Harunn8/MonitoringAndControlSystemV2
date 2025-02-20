using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using McsCore.Entities;
using McsApplication.Models;
using McsApplication.Responses;

namespace McsApplication.Mapper
{
    public class McsMappingProfile : Profile
    {
        public McsMappingProfile()
        {
            // Entites to Model
            CreateMap<DeviceData, DeviceDataModel>();
            CreateMap<SnmpDevice, SnmpDeviceModel>();
            CreateMap<TcpDevice, TcpDeviceModel>();
            CreateMap<User, UserModel>();

            // Model to Response
            CreateMap<DeviceDataModel, DeviceDataResponse>();
            CreateMap<SnmpDeviceModel, SnmpDeviceResponse>();
            CreateMap<TcpDeviceModel, TcpDeviceResponse>();
            CreateMap<UserModel, UserResponse>();

            // Entities to Response
            CreateMap<DeviceData, DeviceDataResponse>();
            CreateMap<SnmpDevice, SnmpDeviceResponse>();
            CreateMap<TcpDevice, TcpDeviceResponse>();
            CreateMap<User, UserResponse>();

        }
    }
}
