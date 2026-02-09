using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FutureTime.MongoDB.Model
{
    public class DailyCounterModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public int counter { get; set; }
    }
}
