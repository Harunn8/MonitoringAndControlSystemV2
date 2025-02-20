using System;
using Microsoft.AspNetCore.Mvc;
using Services;
using McsApplication.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Sockets;
using EventBusMqtt.Producer;
using MQTTnet.Protocol;
using Serilog.Core;
using System.Globalization;
using McsCore.Entities;

namespace McsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TcpDeviceController : ControllerBase
    {
        private readonly TcpService _tcpService;
        private readonly Logger _logger;

        public TcpDeviceController(TcpService tcpService, Logger logger)
        {
            _tcpService = tcpService;
            _logger = logger;
        }

        [HttpGet("GetAllTcpDevice")]
        public async Task<IActionResult> GetAllTcpDevice()
        {
            var device = await _tcpService.GetTcpDeviceAsync();
            return Ok(device);
        }

        [HttpGet("GetTcpDeviceById")]
        public async Task<IActionResult> GetTcpDeviceById(string id)
        {
            var device = await _tcpService.GetTcpDeviceById(id);
            if (device == null) 
            {
                return BadRequest("Device not found");
            }
            return Ok(device);
        }

        [HttpGet("GetTcpDeviceByIpAndPort")]
        public async Task<IActionResult> GetTcpDeviceByIpAndPort(string ipAddress, int port)
        {
            var device = await _tcpService.GetTcpDeviceByIpAndPort(ipAddress, port);
            if (device == null)
            {
                return BadRequest("Device not found");
            }
            return Ok(device);
        }

        [HttpPost("AddTcpDevice")]
        public async Task<IActionResult> AddTcpDevice([FromBody] TcpDevice tcpDevice)
        {
            if (tcpDevice == null)
            {
                return BadRequest("Invalid device data");
            }

            await _tcpService.AddTcpDevice(tcpDevice);
            return Ok("Device added successful");
        }

        [HttpPut("UpdateTcpDevice")]
        public async Task<IActionResult> UpdateTcpDevice(string id, TcpDevice updatedTcpDevice)
        {
            var device = await _tcpService.GetTcpDeviceById(id);

            if(device == null)
            {
                return BadRequest("Device not found");
            }

            await _tcpService.UpdateTcpDevice(id, updatedTcpDevice);
            return Ok("Device updated");
        }

        [HttpDelete("Delete Tcp Device")]
        public async Task<IActionResult> DeleteTcpDevice(string id)
        {
            var device = await _tcpService.GetTcpDeviceById(id);

            if (device == null)
            {
                return BadRequest("Device not found");
            }

            await _tcpService.DeleteTcpDevice(id);
            return Ok("Device deleted successful");
        }

        [HttpPost("StartTcpCommunication")]
        public async Task<IActionResult> StartTcpCommunication(string ipAddress, int port)
        {
            var device = await _tcpService.GetTcpDeviceByIpAndPort(ipAddress, port);

            if (device == null)
            {
                return BadRequest("Device not found");
            }
            
            string tcpFormat = device.TcpFormat != null ? string.Join(",", device.TcpFormat) : string.Empty;

            await _tcpService.StartCommunication(device.IpAddress,device.Port, tcpFormat, data => Console.WriteLine($"Raw Data : {data}"),
                new System.Threading.CancellationToken());

            return Ok("Communication Started");
        }

        [HttpPost("StopTcpCommunication")]
        public async Task<IActionResult> StopTcpCommunication(TcpClient tcpClient)
        {
            await _tcpService.StopCommunication(tcpClient);
            return Ok("Communication stopped");
        }
    }
}
