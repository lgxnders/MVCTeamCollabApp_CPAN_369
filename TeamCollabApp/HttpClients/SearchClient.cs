using System.Text.Json;

namespace TeamCollabApp.HttpClients
{
    public class SearchClient(HttpClient httpClient, IConfiguration configuration)
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private string ApiKey => configuration["Services:SearchApiKey"] ?? string.Empty;

        public async Task<T?> SearchAsync<T>(string query, string userId)
        {
            var encodedQuery = Uri.EscapeDataString(query);
            var encodedUserId = Uri.EscapeDataString(userId);

            var request = new HttpRequestMessage(HttpMethod.Get, $"search?q={encodedQuery}&userId={encodedUserId}");
            request.Headers.Add("X-Api-Key", ApiKey);

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
    }
}
