using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace FutureTime.MongoDB.Model
{
    public class GuestsModel : MasterModel
    {
        //[BsonElement("items")]
        //[JsonPropertyName("items")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string city_id { get; set; }
        public decimal tz { get; set; }
        public string dob { get; set; }
        public string tob { get; set; }
        public string? token { get; set; }
        public string? otp { get; set; }
        public string? gender { get; set; }
        public bool active { get; set; }
        public string device_token { get; set; }
        public string device_type { get; set; }
        public GuestsProfileModel? guest_profile { get; set; }
        public CityListModal city { get; set; }
        public string api_planet_detail { get; set; }
    }

}
