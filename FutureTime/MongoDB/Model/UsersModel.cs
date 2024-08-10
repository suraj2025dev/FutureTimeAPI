using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace FutureTime.MongoDB.Model
{
    public class UsersModel
    {
        //[BsonElement("items")]
        //[JsonPropertyName("items")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public int user_type_id { get; set; }
        public bool active { get; set; }
    }

}
