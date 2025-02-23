using System;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using Services.Base;
using McsCore.Entities;
using AutoMapper;
using McsApplication.Responses;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _user;
        private static readonly string key = "BuSadeceBirOrnekAnahtar12345_301";
        private static readonly string IV = "OrnekIV123456789";
        private readonly IMapper _mapper;

        public UserService(IMongoDatabase database, IMapper mapper)
        {
            _user = database.GetCollection<User>("User");
            _mapper = mapper;
        }

        public async Task<List<User>> GetUsersAsync()
        {
           return await _user.Find(user => true).ToListAsync();
        }

        public async Task<User> GetUserById(string id)
        {
           return await _user.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddUser(User user)
        {
            user.Password = Encrypt(user.Password);
            await _user.InsertOneAsync(user);
        }

        public async Task UpdateUser(string id, User updatedUser)
        {
            await _user.ReplaceOneAsync(u => u.Id == id, updatedUser);
        }

        public async Task DeleteUser(string id)
        {
            await _user.DeleteOneAsync(u => u.Id == id);
        }

        public async Task<User> GetUserByUserNameAndPasswordAsync(string userName, string password)
        {
            var user = await _user.Find(u => u.UserName == userName && u.Password == password).FirstOrDefaultAsync();
            if (user != null)
            {
                user.Password = Decrypt(user.Password);
            }
            else if (user.Password != password)
            {
                return null;
            }
            return user;
        }

        public string Encrypt(string plainText)
        {
            byte[] keys = Encoding.UTF8.GetBytes(key);
            byte[] iv = Encoding.UTF8.GetBytes(IV);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keys;
                aes.IV = iv;

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream,aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (var writer = new StreamWriter(cryptoStream, Encoding.UTF8, 1024, leaveOpen: true))
                        {
                            writer.Write(plainText);
                        }
                        cryptoStream.FlushFinalBlock();
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }

        public string Decrypt(string encryptedText)
        {
            byte[] keys = Encoding.UTF8.GetBytes(key);
            byte[] iv = Encoding.UTF8.GetBytes(IV);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keys;
                aes.IV = iv;

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream =
                           new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (var reader = new StreamReader(cryptoStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public async Task<User> GetUserByUserName(string name)
        {
            var user =  await _user.Find(u => u.UserName == name).FirstOrDefaultAsync();
            return user;
        }
    }
}