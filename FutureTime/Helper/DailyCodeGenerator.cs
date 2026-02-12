using FutureTime.MongoDB;
using FutureTime.MongoDB.Model;
using MongoDB.Driver;

namespace FutureTime.Helper
{
    public class DailyCodeGenerator
    {
        public string GenerateDailyCode()
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");

            var col = MongoDBService.ConnectCollection<DailyCounterModel>(MongoDBService.COLLECTION_NAME.DailyCounterModel);
            var filters = Builders<DailyCounterModel>.Filter.And(
                                    Builders<DailyCounterModel>.Filter.Eq(x => x.date, today));

            var update = Builders<DailyCounterModel>.Update
                .SetOnInsert(x => x.date, today)
                .Inc(x => x.counter, 1);

            var options = new FindOneAndUpdateOptions<DailyCounterModel>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            var result = col.FindOneAndUpdate(filters, update, options);
            return $"{today}{result.counter:D4}";
        }
    }
}
