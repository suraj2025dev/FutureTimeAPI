using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace FutureTime.MongoDB.Model
{
    public class QuestionModel
    {
        //[BsonElement("items")]
        //[JsonPropertyName("items")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public string question { get; set; }
        public int order_id { get; set; }
        public string question_category_id { get; set; }
        public bool active { get; set; }
        public decimal price { get; set; }
    }

}
