using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace FutureTime.MongoDB.Data
{
    public class GuestsProfileDTO
    {
        public string guest_id { get; set; }
        public string basic_description { get; set; }
        public string lucky_number { get; set; }
        public string lucky_gem { get; set; }
        public string lucky_color { get; set; }
        public int rashi_id { get; set; }
    }

}
