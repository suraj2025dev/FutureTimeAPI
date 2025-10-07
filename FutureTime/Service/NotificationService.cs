using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace FutureTime.Service
{
    public class NotificationService
    {
        private readonly string _projectId;
        private readonly string _serviceAccountFile;
        private readonly HttpClient _httpClient;

        public NotificationService(IConfiguration configuration)
        {
            _projectId = configuration["Firebase:ProjectId"]!;
            _serviceAccountFile = configuration["Firebase:ServiceAccountPath"]!;
            _httpClient = new HttpClient();
        }

        public async Task<bool> SendPushNotificationAsync(string targetToken, string title, string body)
        {
            try
            {
                // 1. Load service account credentials

                GoogleCredential credential;
                using var stream = new FileStream(_serviceAccountFile, FileMode.Open, FileAccess.Read);
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

                // 2. Get access token
                var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

                // 3. Prepare the request
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var message = new
                {
                    message = new
                    {
                        token = targetToken,
                        notification = new
                        {
                            title,
                            body
                        },
                        data = new
                        {
                            click_action = "FLUTTER_NOTIFICATION_CLICK",
                            status = "done"
                        }
                    }
                };

                var jsonMessage = JsonConvert.SerializeObject(message);
                var content = new StringContent(jsonMessage, System.Text.Encoding.UTF8, "application/json");

                // 4. Send the request
                var response = await _httpClient.PostAsync(
                    $"https://fcm.googleapis.com/v1/projects/{_projectId}/messages:send", content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ Notification sent successfully!");
                    return true;
                }
                else
                {
                    Console.WriteLine($"❌ Failed to send notification. Response: {responseContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                return false;
            }
        }
    }
}
