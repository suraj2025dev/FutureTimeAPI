using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using global::MongoDB.Driver;
using FutureTime.MongoDB.Model;
using System.Drawing;
using Library.Data;

namespace FutureTime.MongoDB
{
    
    public static class MongoDBService
    {

        //private readonly IMongoCollection<DailyKundaliUpdates> dailyKundaliUpdates;

        public enum COLLECTION_NAME
        {
            DailyKundaliUpdates,
            UsersModel,
            QuestionCategoryModel
        }

        public static IMongoCollection<T>ConnectCollection<T>(COLLECTION_NAME collection_name)
        {
            MongoClient client = new MongoClient(AppStatic.CONFIG.App.MongoDB.ConnectionURL);//connection string
            IMongoDatabase database = client.GetDatabase((AppStatic.CONFIG.App.MongoDB.DatabaseName));//db name
            return database.GetCollection<T>(Enum.GetName(typeof(COLLECTION_NAME), collection_name));
        }


    }
}
