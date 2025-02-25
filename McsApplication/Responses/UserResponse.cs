using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McsApplication.Models;
using McsCore.Entities;
using McsInfrastructure.Data;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using AutoMapper;

namespace McsApplication.Responses
{
    public class UserResponse
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime LoginDate { get; set; }
    }
}
