using Microsoft.AspNetCore.Mvc;
using Services;
using Serilog;
using Serilog.Core;

namespace McsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly LoginService _loginService;
        private readonly Logger _logger;

        public LoginController(LoginService loginService, Logger logger)
        {
            _loginService = loginService;
            _logger = logger;
        }

        [HttpPost("Authenticate")]
        public async Task<IActionResult> Authenticate(LoginRequest loginRequest)
        {
            var validateUser = await _loginService.ValidateUser(loginRequest.Username, loginRequest.Password);

            if (validateUser)
            {
                var token = _loginService.GenerateJwtToken(loginRequest.Username);
                _logger.Information($"{loginRequest.Username} login at {DateTime.Now}");
                return Ok(new { Token = token });
            }

            return Unauthorized("Invalid username or password");
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
