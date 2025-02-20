using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using McsApplication.Models;
using McsApplication.Services.Base;
using McsApplication.Services;

namespace Services
{
    public class LoginService : ILoginService
    {
        private readonly UserService _userService;
        private readonly string _jwtToken;

        public LoginService(UserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _jwtToken = configuration["jwt:Key"];
        }

        public string GenerateJwtToken(string userName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtToken);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userName) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> ValidateUser(string userName, string password)
        {
            var user = await _userService.GetUserByUserNameAndPasswordAsync(userName, password);
            return user != null;
        }
    }
}
