namespace XrayServerAPI.ApiKey;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string API_KEY_HEADER_NAME = "X-API-KEY";
    private readonly string _apiKey;

    public ApiKeyMiddleware(RequestDelegate next, ApiKeyOptions options)
    {
        _next = next;
        _apiKey = options.ApiKey;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/")
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key missing");
            return;
        }

        if (!_apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        await _next(context);
    }
}
