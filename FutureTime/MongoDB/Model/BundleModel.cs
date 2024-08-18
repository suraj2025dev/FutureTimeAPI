
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace FutureTime.MongoDB.Model
{
    public class BundleModel
    {
        //[BsonElement("items")]
        //[JsonPropertyName("items")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string image_blob { get; set; }
        public DateTime effective_from { get; set; }
        public DateTime effective_to { get; set; }
        public bool active { get; set; }
        public decimal price { get; set; }
        public int horoscope_question_count { get; set; }
        public int compatibility_question_count { get; set; }
        public string auspicious_question_id { get; set; }
    }

}
