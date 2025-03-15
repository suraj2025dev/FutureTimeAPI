using FutureTime.MongoDB;
using FutureTime.MongoDB.Model;
using Library.Data;
using System.Text.Json;

namespace FutureTime.Helper
{
    public static class VedicAPIConnection
    {
        public static class APICall
        {
            public static async Task<JsonElement> GetPlanetDetail(
                 DateTime dateOfBirth,
                string timeOfBirth,
                string latitude,
                string longitude,
                string timezone,
                string language = "en")
            {

               
                var requestUrl = $"{AppStatic.CONFIG.App.VedicAPI.URL}/horoscope/planet-details?dob={dateOfBirth.ToString("dd/MM/yyyy")}&tob={timeOfBirth}" +
                                 $"&lat={latitude}&lon={longitude}&tz={timezone}&lang={language}&api_key={AppStatic.CONFIG.App.VedicAPI.apiKey}";

                JsonDocument doc;
                using (HttpClient httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();
                    doc = JsonDocument.Parse(content);
                }

                var col = MongoDBService.ConnectCollection<APICallLogModel>(MongoDBService.COLLECTION_NAME.APICallLogModel);
                col.InsertOne(new APICallLogModel { 
                    request_url = requestUrl,
                    response= doc.RootElement.Clone().ToString()
                });

                return doc.RootElement.Clone();
            }
        }
    }
}
