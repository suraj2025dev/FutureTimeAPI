using Newtonsoft.Json;

namespace APIConnection
{
    public static class APICall
    {
        public static async Task<dynamic> GetRequest(string URL)
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(URL))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<dynamic>(apiResponse);
                    return res;
                }
            }
        }
    }
}