using System;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace XrayServerAPI.Xray;

public class XrayManager
{
    private static readonly object _lock = new();

    public XrayKey CreateKey()
    {
        lock (_lock)
        {
            var key = GenerateKey();

            var path = "/home/XrayServerAPI/out/xrayconf.json";

            var json = File.ReadAllText(path);

            var config = System.Text.Json.JsonSerializer.Deserialize<XrayConfig>(json,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new Exception("Failed parse xray config");

            var inbound = config.Inbounds
                .FirstOrDefault(x => x.Protocol == "vless");

            if (inbound == null)
                throw new Exception("VLESS inbound not found");

            inbound.Settings.Clients.Add(new Client
            {
                Id = key.Id
            });

            var newJson = System.Text.Json.JsonSerializer.Serialize(config,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(path, newJson);

            RestartXray();

            return key;
        }
    }

    public XrayKey? DeleteKey(string id)
    {
        lock (_lock)
        {
            var path = "/home/XrayServerAPI/out/xrayconf.json";

            var json = System.IO.File.ReadAllText(path);

            var config = System.Text.Json.JsonSerializer.Deserialize<XrayConfig>(json,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new Exception("Failed parse xray config");

            var inbound = config.Inbounds
                .FirstOrDefault(x => x.Protocol == "vless");

            if (inbound == null)
                throw new Exception("VLESS inbound not found");

            inbound.Settings.Clients ??= new List<Client>();

            var client = inbound.Settings.Clients
                .FirstOrDefault(x => x.Id == id);

            if (client == null)
                return null;

            inbound.Settings.Clients.Remove(client);

            var newJson = System.Text.Json.JsonSerializer.Serialize(config,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

            System.IO.File.WriteAllText(path, newJson);

            RestartXray();

            return GetKey(id);
        }
    }

    public bool HasKey(string id)
    {
        var path = "/home/XrayServerAPI/out/xrayconf.json";

        var json = System.IO.File.ReadAllText(path);

        var config = System.Text.Json.JsonSerializer.Deserialize<XrayConfig>(json,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Failed parse xray config");

        var inbound = config.Inbounds
            .FirstOrDefault(x => x.Protocol == "vless");

        if (inbound == null)
            return false;

        return inbound.Settings.Clients?
            .Any(x => x.Id == id) == true;
    }

    public List<XrayKey> GetKeys()
    {
        lock (_lock)
        {
            var path = "/home/XrayServerAPI/out/xrayconf.json";

            var json = File.ReadAllText(path);

            var config = System.Text.Json.JsonSerializer.Deserialize<XrayConfig>(json,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new Exception("Failed parse xray config");

            var inbound = config.Inbounds
                .FirstOrDefault(x => x.Protocol == "vless");

            if (inbound == null)
                return new List<XrayKey>();

            if (inbound.Settings.Clients == null)
                return new List<XrayKey>();

            var result = inbound.Settings.Clients
                .Select(c => GetKey(c.Id))
                .ToList();

            return result;
        }
    }

    private XrayKey GenerateKey()
    {
        var uuid = Guid.NewGuid().ToString();
        return GetKey(uuid);
    }

    private XrayKey GetKey(string uuid)
    {
        var dataPath = "/home/XrayServerAPI/out/data/xray_data.json";
        var json = System.IO.File.ReadAllText(dataPath) ??
            throw new Exception("Xray data file not found");

        var data = System.Text.Json.JsonSerializer.Deserialize<XrayData>(json)
            ?? throw new Exception("Failed get XrayData");

        var host = data.Domain;
        var port = 1443;

        var accessKey =
            $"vless://{uuid}@{host}:{port}" +
            $"?encryption=none" +
            $"&type=raw" +
            $"&security=reality" +
            $"&fp=chrome" +
            $"&sni=speed.cloudflare.com" +
            $"&pbk={data.Hash32}" +
            $"&sid={data.ShortId}" +
            $"#divpn";

        return new XrayKey
        {
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
