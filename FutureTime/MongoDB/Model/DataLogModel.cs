
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;


namespace FutureTime.MongoDB.Model
{
    public class DataLogModel
    {
        //[BsonElement("items")]
        //[JsonPropertyName("items")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public string? collection_id { get; set; }
        public string from_collection { get; set; }
        public BsonDocument data{get;set;}
        public DateTime created_date { get; set; }
        public string created_by { get; set; }
    }

}
