namespace XrayServerAPI.Services;

public class ApiKeyService
{
    public string ApiKey { get; }

    public ApiKeyService()
    {
        var key = Environment.GetEnvironmentVariable("API_KEY");

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new Exception("API_KEY is not set in environment variables");
        }

        ApiKey = key;
    }
}
