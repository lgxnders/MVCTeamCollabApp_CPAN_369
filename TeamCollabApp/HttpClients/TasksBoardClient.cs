using System.Text;
using System.Text.Json;

namespace TeamCollabApp.HttpClients
{
    public class TasksBoardClient(HttpClient httpClient, IConfiguration configuration)
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private string ApiKey => configuration["Services:TasksApiKey"] ?? string.Empty;

        private HttpRequestMessage BuildRequest(HttpMethod method, string path, string userId, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, path);
            request.Headers.Add("X-User-Id", userId);
            request.Headers.Add("X-Api-Key", ApiKey);
            if (content != null) request.Content = content;
            return request;
        }

        private static StringContent ToJson(object payload) =>
            new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        public async Task<T?> GetAsync<T>(string path, string userId)
        {
            var request = BuildRequest(HttpMethod.Get, path, userId);
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        public async Task<HttpResponseMessage> PostAsync(string path, object payload, string userId)
        {
            var request = BuildRequest(HttpMethod.Post, path, userId, ToJson(payload));
            return await httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> PutAsync(string path, object payload, string userId)
        {
            var request = BuildRequest(HttpMethod.Put, path, userId, ToJson(payload));
            return await httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string path, string userId)
        {
            var request = BuildRequest(HttpMethod.Delete, path, userId);
            return await httpClient.SendAsync(request);
        }
    }
}
