using Library.Data;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Json;

namespace APIConnection
{
    public static class APICall
    {
        public static async Task<JsonElement> GetPlanetDetails(
            DateTime dateOfBirth,
            string timeOfBirth,
            string latitude,
            string longitude,
            string timezone,
            string language = "en")
        {
            var requestUrl = $"{AppStatic.CONFIG.App.VedicAPI.URL}/horoscope/planet-details?dob={dateOfBirth.ToString("dd/MM/yyyy")}&tob={timeOfBirth}" +
                             $"&lat={latitude}&lon={longitude}&tz={timezone}&lang={language}&api_key={AppStatic.CONFIG.App.VedicAPI.apiKey}";

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("VEDIC API:");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("URL: "+ requestUrl);
            


            JsonDocument doc;
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                doc = JsonDocument.Parse(content);
            }

            Console.WriteLine("RESPONSE: " );
            Console.WriteLine("" + doc.RootElement.Clone().ToString());
            return doc.RootElement.Clone();
        }
    }
}