using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using global::MongoDB.Driver;
using FutureTime.MongoDB.Model;
using System.Drawing;

namespace FutureTime.MongoDB
{
    
    public static class MongoDBService
    {

        //private readonly IMongoCollection<DailyKundaliUpdates> dailyKundaliUpdates;

        public enum COLLECTION_NAME
        {
            DailyKundaliUpdates
        }

        public static IMongoCollection<T>ConnectCollection<T>(COLLECTION_NAME collection_name)
        {
            MongoClient client = new MongoClient("mongodb://localhost:27017");//connection string
            IMongoDatabase database = client.GetDatabase("FutureTime");//db name
            return database.GetCollection<T>(Enum.GetName(typeof(COLLECTION_NAME), collection_name));
        }


    }
}
