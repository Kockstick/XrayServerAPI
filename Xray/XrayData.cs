using System.Text.Json.Serialization;

namespace XrayServerAPI.Xray;

public class XrayData
{
    [JsonPropertyName("domain")]
    public string Domain { get; set; }

    [JsonPropertyName("privateKey")]
    public string PrivateKey { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("hash32")]
    public string Hash32 { get; set; }   
    
    [JsonPropertyName("shortId")]
    public string ShortId { get; set; }

    public string PublicKey => Password;
}
