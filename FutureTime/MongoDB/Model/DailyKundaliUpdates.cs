using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace FutureTime.MongoDB.Model
{
    public class DailyKundaliUpdates
    {
        //[BsonElement("items")]
        //[JsonPropertyName("items")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }

        public string transaction_date { get; set; }
        public List<DailyKundaliUpdatesDetail> items { get; set; }
    }

    public class DailyKundaliUpdatesDetail
    {
        public int rashi_id { get; set; }

        public int rating { get; set; }
        public string description { get; set; }
    }
}
