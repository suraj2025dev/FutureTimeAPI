using FutureTime.MongoDB.Model;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;

namespace FutureTime.MongoDB
{
    public static class MongoLogRecorder
    {
        public static async Task RecordLogAsync<T>(MongoDBService.COLLECTION_NAME collection_name, string _id, string user_id)
        {
            var to_log_data = await MongoDBService.ConnectCollection<T>(collection_name)
                            .Find(Builders<T>.Filter.Eq("_id",ObjectId.Parse(_id))).FirstOrDefaultAsync();
            if (to_log_data != null)
            {
                var col = MongoDBService.ConnectCollection<DataLogModel>(MongoDBService.COLLECTION_NAME.DataLogModel);
                col.InsertOne(new DataLogModel { 
                    data= BsonDocument.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(to_log_data)),
                    from_collection = Enum.GetName(typeof(MongoDBService.COLLECTION_NAME), collection_name),
                    collection_id= _id,
                    created_by= user_id,
                    created_date= DateTime.Now
                });
            }
        }
    }
}
