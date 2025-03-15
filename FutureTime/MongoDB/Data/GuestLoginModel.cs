using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace FutureTime.MongoDB.Model
{
    public class GuestLoginData
    {
       
        public string name { get; set; }
        public string email { get; set; }
        public string city_id { get; set; }
        public decimal gmt { get; set; }
        public string dob { get; set; }
        public string tob { get; set; }
        public bool is_login { get; set; }
    }

}
