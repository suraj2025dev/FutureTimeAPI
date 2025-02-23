using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace FutureTime.MongoDB.Model
{
    public class QuestionModel : MasterModel
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
        public decimal price { get; set; }//net price after discount

        #region After Bundle & Discount & intial free offerings.
        public decimal price_before_discount { get; set; }//After discount & bundle concept added.
        public bool is_initial_offerings { get; set; }
        public bool is_bundle { get; set; }
        public string? image_blob { get; set; }
        public string? effective_from { get; set; }
        public string? effective_to { get; set; }
        public decimal discount_amount { get; set; }
        #endregion
    }

}
