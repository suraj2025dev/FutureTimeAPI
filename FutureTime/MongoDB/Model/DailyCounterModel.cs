using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Specialized;

namespace FutureTime.MongoDB.Model
{
    public class DailyCounterModel
    {
        [BsonId]
        public string date { get; set; } = default!;
        public int counter { get; set; }
    }
}
