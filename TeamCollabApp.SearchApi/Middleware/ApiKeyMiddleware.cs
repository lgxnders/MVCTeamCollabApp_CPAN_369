namespace TeamCollabApp.SearchApi.Middleware
{
    public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        private const string ApiKeyHeader = "X-Api-Key";

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var receivedKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("API key missing.");
                return;
            }

            var expectedKey = configuration["ServiceApiKey"];
            if (receivedKey != expectedKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid API key.");
                return;
            }

            await next(context);
        }
    }
}
