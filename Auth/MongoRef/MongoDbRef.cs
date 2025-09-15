using Library.Data;
using MongoDB.Driver;

namespace Auth.MongoRef
{
    public class MongoDbRef
    {
        public enum COLLECTION_NAME
        {
            UsersModel
        }

        public static IMongoCollection<T> ConnectCollection<T>(COLLECTION_NAME collection_name)
        {
            MongoClient client = new MongoClient(AppStatic.CONFIG.App.MongoDB.ConnectionURL);//connection string
            IMongoDatabase database = client.GetDatabase(AppStatic.CONFIG.App.MongoDB.DatabaseName);//db name
            return database.GetCollection<T>(Enum.GetName(typeof(COLLECTION_NAME), collection_name));
        }
    }
}
