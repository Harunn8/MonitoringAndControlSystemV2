using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McsApplication.Models;
using McsApplication.Responses;
using McsCore.Entities;

namespace Services.Base
{
    public interface IUserService
    {
        public Task<List<User>> GetUsersAsync();
        public Task<User> GetUserById(string id);
        public Task AddUser(User user);
        public Task UpdateUser(string id ,User user);
        public Task DeleteUser(string id);
        public Task<User> GetUserByUserNameAndPasswordAsync(string userName, string password);
        public string Encrypt(string plainText);
        public string Decrypt(string encryptedText);
    }
}
