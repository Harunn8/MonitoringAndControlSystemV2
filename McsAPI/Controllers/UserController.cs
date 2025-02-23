using Microsoft.AspNetCore.Mvc;
using McsApplication.Models;
using System.Threading.Tasks;
using McsApplication.Services;
using Services;
using EventBusMqtt.Producer;
using Serilog;
using McsCore.Entities;

namespace McsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private MqttProducer _mqttProducer;

        public UserController(UserService userService, MqttProducer mqttProducer)
        {
           _userService = userService;
            _mqttProducer = mqttProducer;
        }

        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetAllUser()
        {
            var user = await _userService.GetUsersAsync();
            return Ok(user);
        }

        [HttpGet("GetUserById")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserById(id);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            return Ok(user);
        }

        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            if (user == null) 
            {
                return BadRequest("Invalid user data");
            }

            await _userService.AddUser(user);
            return Ok("User added");
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User updatedUser)
        {
            var user = await _userService.GetUserById(id);
            if (user == null) 
            {
                return BadRequest("User not found");
            }

            await _userService.UpdateUser(id, updatedUser);
            return Ok("User updated");
        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userService.GetUserById(id);

            if(user == null)
            {
                return BadRequest("User not found");
            }

            await _userService.DeleteUser(id);
            return Ok("User deleted");
        }

        [HttpGet("GetUserByName")]
        public async Task<IActionResult> GetUserByName(string name)
        {
            var user = await _userService.GetUserByUserName(name);
            if (user == null)
            {
                return BadRequest("user not found");
            }
            return Ok(user);
        }
    }
}
