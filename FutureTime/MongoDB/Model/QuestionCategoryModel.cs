using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace FutureTime.MongoDB.Model
{
    public class QuestionCategoryModel : MasterModel
    {
        //[BsonElement("items")]
        //[JsonPropertyName("items")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public string category { get; set; }
        public int order_id { get; set; }
        public int category_type_id { get; set; }
        public bool active { get; set; }
    }

}
