using Library.Data;
using MongoDB.Driver;

namespace FutureTime.MongoDB
{
    
    /// <summary>
    /// Provides methods for connecting to MongoDB collections and defines collection names used in the application.
    /// </summary>
    public static class MongoDBService
    {   
        /// <summary>
        /// Represents the names of MongoDB collections used in the application.
        /// </summary>
        public enum COLLECTION_NAME
        {
            /// <summary>
            /// Represents the collection for daily horoscope updates.
            /// </summary>
            DailyHoroscopeUpdatesModel,
            /// <summary>
            /// Represents the collection for users.
            /// </summary>
            UsersModel,
            /// <summary>
            /// Represents the collection for question categories.
            /// </summary>
            QuestionCategoryModel,
            /// <summary>
            /// Represents the collection for questions.
            /// </summary>
            QuestionModel,
            /// <summary>
            /// Represents the collection for daily auspicious time updates.
            /// </summary>
            DailyAuspiciousTimeUpdateModel,
            /// <summary>
            /// Represents the collection for guests.
            /// </summary>
            GuestsModel,
            /// <summary>
            /// Represents the collection for cities. (Not in use)
            /// </summary>
            CitiesModel,//NOT IN USE
            /// <summary>
            /// Represents the collection for daily compatibility updates.
            /// </summary>
            DailyCompatibilityUpdateModel,
            /// <summary>
            /// Represents the collection for bundles.
            /// </summary>
            BundleModel,
            /// <summary>
            /// Represents the collection for data logs.
            /// </summary>
            DataLogModel,
            /// <summary>
            /// Represents the collection for starting inquiry processes.
            /// </summary>
            StartInquiryProcessModel,
            /// <summary>
            /// Represents the collection for city list.
            /// </summary>
            CityListModal,
            /// <summary>
            /// Represents the collection for API call logs.
            /// </summary>
            APICallLogModel,
            /// <summary>
            /// Represents the collection for rejected inquiries.
            /// </summary>
            RejectInquiryModel,
            /// <summary>
            /// Represents the collection for daily counter data.
            /// </summary>
            DailyCounterModel,
        }

        /// <summary>
        /// Connects to the specified MongoDB collection and returns a typed collection interface.
        /// </summary>
        /// <typeparam name="T">The type representing the collection's documents.</typeparam>
        /// <param name="collection_name">The name of the collection to connect to.</param>
        /// <returns>An <see cref="IMongoCollection{T}"/> for the specified collection.</returns>
        public static IMongoCollection<T> ConnectCollection<T>(COLLECTION_NAME collection_name)
        {
            MongoClient client = new MongoClient(AppStatic.CONFIG.App.MongoDB.ConnectionURL);//connection string
            IMongoDatabase database = client.GetDatabase((AppStatic.CONFIG.App.MongoDB.DatabaseName));//db name
            return database.GetCollection<T>(Enum.GetName(typeof(COLLECTION_NAME), collection_name));
        }
    }
}
