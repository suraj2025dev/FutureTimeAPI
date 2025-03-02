namespace FutureTime.Service
{
    using Amazon.Runtime.Internal;
    using FirebaseAdmin;
    using FirebaseAdmin.Messaging;
    using FutureTime.MongoDB.Model;
    using FutureTime.MongoDB;
    using global::MongoDB.Bson;
    using global::MongoDB.Driver;
    using Google.Apis.Auth.OAuth2;
    using Microsoft.Extensions.Options;
    using Org.BouncyCastle.Asn1.Ocsp;
    using System.Threading.Tasks;

    public class FirebaseService
    {

        private readonly FirebaseApp _firebaseApp;

        public FirebaseService()
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                _firebaseApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("ftpushnotification-e9446-firebase-adminsdk-fbsvc-13efe52a0c.json"),
                });
            }
            else
            {
                _firebaseApp = FirebaseApp.DefaultInstance;
            }
        }

        public async Task<bool> PushNotificationAsync(string title, string body, Dictionary<string,string> dict, string guest_id) {
            if (dict == null)
            {
                dict = new Dictionary<string, string>();
            }

            if (guest_id == null) {
                return false;
            }

            #region GetGuestProfileData
            var col = MongoDBService.ConnectCollection<GuestsModel>(MongoDBService.COLLECTION_NAME.GuestsModel);

            var obj_id = new ObjectId(guest_id);

            var filter = Builders<GuestsModel>.Filter.Eq("_id", obj_id);
            var item = await col.Find(filter).FirstOrDefaultAsync();
            if(item.device_token == null)
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

            var messaging = FirebaseMessaging.DefaultInstance;
            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return true;
        }
    }

}
