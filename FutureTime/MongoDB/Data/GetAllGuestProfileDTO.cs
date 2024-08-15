using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace FutureTime.MongoDB.Data
{
    public class GetAllGuestProfileDTO
    {
        public string name { get; set; }
        public string email { get; set; }
        public string city_id { get; set; }
        public string dob { get; set; }
        public string tob { get; set; }
        public bool? is_profile_verified { get; set; }
        public int page_number { get; set; }
        public int page_size { get; set; }
    }

}
