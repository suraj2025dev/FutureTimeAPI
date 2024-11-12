using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace Auth.MongoRef
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
        public DateTime updated_date { get; set; }
        public string updated_by { get; set; }
        public DateTime created_date { get; set; }
        public string created_by { get; set; }
    }

}
