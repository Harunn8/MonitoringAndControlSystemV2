using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McsApplication.Models;
using McsApplication.Responses;

namespace McsApplication.Services.Base
{
    public interface ILoginService
    {
        public Task<bool> ValidateUser(string userName, string password);

        public string GenerateJwtToken(string userName);
    }
}
