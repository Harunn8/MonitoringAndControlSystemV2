using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Services;
using McsCore.Entities;

namespace McsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SnmpDeviceController : ControllerBase
    {
        private readonly SnmpDeviceService _snmpDeviceService;
        private readonly ILogger _logger;

        public SnmpDeviceController(SnmpDeviceService snmpDeviceService, ILogger logger)
        {
            _snmpDeviceService = snmpDeviceService;
            _logger = logger;
        }

        [HttpGet("GetsnmpDevice")]
        public async Task<IActionResult> GetSnmpDevice()
        {
            var device = await _snmpDeviceService.GetSnmpDeviceAsync();
            return Ok(device);
        }

        [HttpGet("GetsnmpDeviceById")]
        public async Task<IActionResult> GetSnmpDeviceById(string id)
        {
            var device = await _snmpDeviceService.GetSnmpDeviceById(id);
            if (device != null)
            {
                return Ok(device);
            }
            return BadRequest("Device not found");
        }

        [HttpPost("AddSnmpDevice")]
        public async Task<IActionResult> AddSnmpDevice([FromBody] SnmpDevice snmpDevice)
        {
            if (snmpDevice == null)
            {
                return BadRequest("Device data is required");
            }

            await _snmpDeviceService.AddSnmpDevice(snmpDevice);
            return Ok("Device added successful");
        }

        [HttpPut("Update Snmp Device")]
        public async Task<IActionResult> UpdateSnmpDevice(string id, SnmpDevice snmpDevice)
        {
            var device = await _snmpDeviceService.GetSnmpDeviceById(id);
            if (device == null)
            {
                return BadRequest("Device not found");
            }

            await _snmpDeviceService.UpdateDevice(id, snmpDevice);
            return Ok("Device updated");
        }

        [HttpDelete("Delete Snmp Device")]
        public async Task<IActionResult> DeleteSnmpDevice(string id)
        {
            var device = await _snmpDeviceService.GetSnmpDeviceById(id);
            if (device == null)
            {
                return BadRequest("Device not found");
            }

            await _snmpDeviceService.DeleteDevice(id);
            return Ok("Device Deleted");
        }

    }
}
