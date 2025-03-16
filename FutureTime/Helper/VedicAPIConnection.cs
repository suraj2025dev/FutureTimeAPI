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


            public static async Task<JsonElement> MatchMaking(
                DateTime dateOfBirth,
                string timeOfBirth,
                string latitude,
                string longitude,
                string timezone,

                DateTime dateOfBirth2,
                string timeOfBirth2,
                string latitude2,
                string longitude2,
                string timezone2,
                string language = "en")
            {


                var requestUrl = "";
                
                requestUrl = $"{AppStatic.CONFIG.App.VedicAPI.URL}/matching/ashtakoot-with-astro-details?" +
                                $"boy_dob={dateOfBirth.ToString("dd/MM/yyyy")}&boy_tob={timeOfBirth}&boy_lat={latitude}&boy_lon={longitude}&boy_tz={timezone}&" +
                                $"girl_dob={dateOfBirth2.ToString("dd/MM/yyyy")}&girl_tob={timeOfBirth2}&girl_lat={latitude2}&girl_lon={longitude2}&girl_tz={timezone2}" +
                                $"&lang={language}&api_key={AppStatic.CONFIG.App.VedicAPI.apiKey}";


                JsonDocument doc;
                using (HttpClient httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();
                    doc = JsonDocument.Parse(content);
                }

                var col = MongoDBService.ConnectCollection<APICallLogModel>(MongoDBService.COLLECTION_NAME.APICallLogModel);
                col.InsertOne(new APICallLogModel
                {
                    request_url = requestUrl,
                    response = doc.RootElement.Clone().ToString()
                });

                return doc.RootElement.Clone();
            }

            public static async Task<JsonElement> GetPanchang(
                     DateTime dateOfBirth,
                    string timeOfBirth,
                    string latitude,
                    string longitude,
                    string timezone,
                    string language = "en")
            {


                var requestUrl = $"{AppStatic.CONFIG.App.VedicAPI.URL}/panchang/panchang?date={dateOfBirth.ToString("dd/MM/yyyy")}" +
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
                col.InsertOne(new APICallLogModel
                {
                    request_url = requestUrl,
                    response = doc.RootElement.Clone().ToString()
                });

                return doc.RootElement.Clone();
            }

            public static async Task<JsonElement> GetMahadasha(
                     DateTime? dateOfBirth,
                    string timeOfBirth,
                    string latitude,
                    string longitude,
                    string timezone,
                    string language = "en")
            {
                if (dateOfBirth == null) dateOfBirth = DateTime.Now;

                var requestUrl = $"{AppStatic.CONFIG.App.VedicAPI.URL}/dashas/current-mahadasha-full?dob={dateOfBirth.ToString("dd/MM/yyyy")}&tob={timeOfBirth}" +
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
                col.InsertOne(new APICallLogModel
                {
                    request_url = requestUrl,
                    response = doc.RootElement.Clone().ToString()
                });

                return doc.RootElement.Clone();
            }

        }
    }

}
