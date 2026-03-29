using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace XrayServerAPI.Xray;

public class XrayManager
{
    private static readonly object _lock = new();
    private const string ConfigPath = "/home/XrayServerAPI/out/xrayconf.json";

    public XrayKey CreateKey()
    {
        lock (_lock)
        {
            var key = GenerateKey();

            var root = LoadJson();

            var clients = GetClientsArray(root);

            clients.Add(new JsonObject
            {
                ["id"] = key.Id
            });

            SaveJson(root);

            RestartXray();

            return key;
        }
    }

    public XrayKey? DeleteKey(string id)
    {
        lock (_lock)
        {
            var root = LoadJson();

            var clients = GetClientsArray(root);

            var client = clients
                .FirstOrDefault(x => x?["id"]?.ToString() == id);

            if (client == null)
                return null;

            clients.Remove(client);

            SaveJson(root);

            RestartXray();

            return GetKey(id);
        }
    }

    public bool HasKey(string id)
    {
        var root = LoadJson();

        var clients = GetClientsArray(root);

        return clients.Any(x => x?["id"]?.ToString() == id);
    }

    public List<XrayKey> GetKeys()
    {
        lock (_lock)
        {
            var root = LoadJson();

            var clients = GetClientsArray(root);

            return clients
                .Where(x => x?["id"] != null)
                .Select(x => GetKey(x!["id"]!.ToString()))
                .ToList();
        }
    }

    private JsonNode LoadJson()
    {
        var json = File.ReadAllText(ConfigPath);
        return JsonNode.Parse(json) ?? throw new Exception("Failed parse json");
    }

    private void SaveJson(JsonNode root)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        File.WriteAllText(ConfigPath, root.ToJsonString(options));
    }

    private JsonArray GetClientsArray(JsonNode root)
    {
        var inbounds = root["inbounds"]?.AsArray()
            ?? throw new Exception("inbounds not found");

        var vless = inbounds
            .FirstOrDefault(x => x?["protocol"]?.ToString() == "vless")
            ?? throw new Exception("VLESS inbound not found");

        var settings = vless["settings"]
            ?? throw new Exception("settings not found");

        var clients = settings["clients"] as JsonArray;

        if (clients == null)
        {
            clients = new JsonArray();
            settings["clients"] = clients;
        }

        return clients;
    }

    private XrayKey GenerateKey()
    {
        var uuid = Guid.NewGuid().ToString();
        return GetKey(uuid);
    }

    private XrayKey GetKey(string uuid)
    {
        var json = File.ReadAllText(ConfigPath);

        var root = JsonNode.Parse(json)
            ?? throw new Exception("Failed parse config");

        var inbounds = root["inbounds"]?.AsArray()
            ?? throw new Exception("inbounds not found");

        var vless = inbounds
            .FirstOrDefault(x => x?["protocol"]?.ToString() == "vless")
            ?? throw new Exception("vless inbound not found");

        var port = vless["port"]?.GetValue<int>()
            ?? throw new Exception("port not found");

        var reality = vless["streamSettings"]?["realitySettings"]
            ?? throw new Exception("realitySettings not found");

        var publicKey = reality["publicKey"]?.ToString()
            ?? throw new Exception("publicKey not found");

        var shortId = reality["shortIds"]?.AsArray()?.FirstOrDefault()?.ToString()
            ?? throw new Exception("shortId not found");

        var host = Environment.GetEnvironmentVariable("DOMAIN")
            ?? throw new Exception("DOMAIN not set");

        var serverName = reality["serverNames"]?.AsArray()?.FirstOrDefault()?.ToString()
            ?? throw new Exception("serverName not found");

        var accessKey =
            $"vless://{uuid}@{host}:{port}" +
            $"?encryption=none" +
            $"&type=tcp" +
            $"&security=reality" +
            $"&fp=chrome" +
            $"&sni={serverName}" +
            $"&pbk={publicKey}" +
            $"&sid={shortId}" +
            $"#divpn";

        return new XrayKey
        {
            Id = uuid,
            Host = host,
            Port = port,
            AccessKey = accessKey
        };
    }

    private void RestartXray()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = "-c \"sudo systemctl restart xray\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new Exception($"Failed restart xray: {error}");
        }
    }
}
