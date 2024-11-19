using FutureTime.MongoDB;
using FutureTime.MongoDB.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FutureTime.Helper
{
    public static class UsersHelper
    {
        public static async Task<List<UsersModel>> GetAllUserAsync()
        {
            var col_user = MongoDBService.ConnectCollection<UsersModel>(MongoDBService.COLLECTION_NAME.UsersModel);
            var items_user = await col_user.Find(new BsonDocument()).ToListAsync();
            return items_user;
        }

        public static string GetUserName(List<UsersModel> userList, string id)
        {
            var user_name =  userList.Where(w => w._id == id).Select(s => s.name).FirstOrDefault();
            return user_name ?? "";
        }
    }
}
