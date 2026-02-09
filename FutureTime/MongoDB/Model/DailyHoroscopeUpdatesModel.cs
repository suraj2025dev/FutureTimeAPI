using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FutureTime.MongoDB.Model
{
    /// <summary>
    /// Represents a daily horoscope update, including transaction date and a list of horoscope details.
    /// </summary>
    public class DailyHoroscopeUpdatesModel : MasterModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the daily horoscope update.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }

        /// <summary>
        /// Gets or sets the transaction date for the horoscope update.
        /// </summary>
        public string transaction_date { get; set; }

        /// <summary>
        /// Gets or sets the list of horoscope update details.
        /// </summary>
        public List<DailyHoroscopeUpdatesDetail> items { get; set; }
    }

    /// <summary>
    /// Represents the details of a daily horoscope update for a specific rashi (zodiac sign).
    /// </summary>
    public class DailyHoroscopeUpdatesDetail
    {
        /// <summary>
        /// Gets or sets the unique identifier for the rashi (zodiac sign).
        /// </summary>
        public int rashi_id { get; set; }

        /// <summary>
        /// Gets or sets the rating for the horoscope.
        /// </summary>
        public decimal rating { get; set; }

        /// <summary>
        /// Gets or sets the description of the horoscope.
        /// </summary>
        public string description { get; set; }
    }
}
