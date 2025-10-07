namespace FutureTime.Service
{
    using FirebaseAdmin;
    using FirebaseAdmin.Messaging;
    using FutureTime.MongoDB;
    using FutureTime.MongoDB.Model;
    using global::MongoDB.Bson;
    using global::MongoDB.Driver;
    using Google.Apis.Auth.OAuth2;
    using System.Threading.Tasks;

    public class FirebaseService
    {

        private static readonly Lazy<FirebaseApp> _firebaseApp = new(() => FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile("firebase-service-account.json"),
        }));

        public static FirebaseApp App => _firebaseApp.Value;

        public async Task<bool> PushNotificationAsync(string title, string body, Dictionary<string, string> dict, string guest_id)
        {
            if (string.IsNullOrEmpty(guest_id)) return false;
            dict ??= new Dictionary<string, string>();

            #region GetGuestProfileData
            var col = MongoDBService.ConnectCollection<GuestsModel>(MongoDBService.COLLECTION_NAME.GuestsModel);
            var obj_id = new ObjectId(guest_id);

            var filter = Builders<GuestsModel>.Filter.Eq("_id", obj_id);
            var item = await col.Find(filter).FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(item?.device_token))
            {
                return false;
            }
            #endregion

            var message = new Message()
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body,
                },
                Data = dict,
                Token = item.device_token,
            };

            try
            {
                var messaging = FirebaseMessaging.GetMessaging(App);
                var response = await messaging.SendAsync(message);
                Console.WriteLine($"✅ Notification Sent: {response}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error sending notification: {ex.Message}");
                return false;
            }
        }
    }

}
