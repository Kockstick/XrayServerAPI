namespace XrayServerAPI.Xray;

public class XrayData
{
    public string Domain { get; set; }
    public string PrivateKey { get; set; }
    public string Hash32 { get; set; }   // PublicKey
    public string ShortId { get; set; }

    public string PublicKey => Hash32;
}
