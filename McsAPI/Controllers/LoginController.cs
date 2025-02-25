using Microsoft.AspNetCore.Mvc;
using Services;
using Serilog;

namespace McsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly LoginService _loginService;

        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost("Authenticate")]
        public async Task<IActionResult> Authenticate(LoginRequest loginRequest)
        {
            var validateUser = await _loginService.ValidateUser(loginRequest.Username, loginRequest.Password);

            if (validateUser)
            {
                var token = _loginService.GenerateJwtToken(loginRequest.Username);
                Log.Information($"{loginRequest.Username} login at {DateTime.Now}");
                return Ok(new { Token = "Bearer " + token });
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
