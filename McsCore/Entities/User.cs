using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace McsCore.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userName")]
        public string UserName { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("loginDate")]
        public DateTime LoginDate { get; set; }
    }
}
