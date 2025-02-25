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

            // Entities to Response
            CreateMap<DeviceData, DeviceDataResponse>();

            CreateMap<SnmpDevice, SnmpDeviceResponse>()
                .ForMember(s => s.DeviceName, opt => opt.MapFrom(src => src.DeviceName))
                .ForMember(s => s.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
                .ForMember(s => s.port, opt => opt.MapFrom(src => src.Port))
                .ForMember(s => s.OidList, opt => opt.MapFrom(src => src.OidList));

            CreateMap<TcpDevice, TcpDeviceResponse>()
                .ForMember(s => s.DeviceName, opt => opt.MapFrom(src => src.DeviceName))
                .ForMember(s => s.IPAddress, opt => opt.MapFrom(src => src.IpAddress))
                .ForMember(s => s.Port, opt => opt.MapFrom(src => src.Port))
                .ForMember(s => s.TcpData, opt => opt.MapFrom(src => src.TcpData))
                .ForMember(s => s.TcpFormat, opt => opt.MapFrom(src => src.TcpFormat));

            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.LoginDate, opt => opt.MapFrom(src => src.LoginDate));
        }
    }
}
