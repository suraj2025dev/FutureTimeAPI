using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace FutureTime.MongoDB.Model
{
    public class GuestsModel
    {
        //[BsonElement("items")]
        //[JsonPropertyName("items")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string city_id { get; set; }
        public string dob { get; set; }
        public string tob { get; set; }
        public Guid? token { get; set; }
        public string? otp { get; set; }
        public bool active { get; set; }
    }

}
