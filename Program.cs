using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.FileProviders;
using System.Security.Cryptography;
using XrayServerAPI.ApiKey;
using XrayServerAPI.Install;
using XrayServerAPI.InstallXray;
using XrayServerAPI.Xray;

var builder = WebApplication.CreateBuilder(args);

var domain = Environment.GetEnvironmentVariable("DOMAIN");
if (string.IsNullOrWhiteSpace(domain))
    throw new Exception("DOMAIN environment variable is not set");

string apiKey = getApi();
builder.Services.AddSingleton(new ApiKeyOptions
{
    ApiKey = apiKey
});

builder.Services.AddScoped<XrayManager>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.WebHost.UseUrls("http://127.0.0.1:5000");

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.UseForwardedHeaders();

app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseMiddleware<ApiKeyMiddleware>();
app.MapControllers();

app.MapGet("/", () => "OK");

if (!File.Exists("installed.flag"))
{
    new InstallXrayManager(domain).Install();
    File.WriteAllText("installed.flag", "ok");
}

var caddyStarter = new CaddyStarter(domain);
caddyStarter.Start();

app.Run();

string getApi()
{
    string apiKey;
    if (!File.Exists("apikey.txt"))
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        apiKey = Convert.ToBase64String(bytes);
        File.WriteAllText("apikey.txt", apiKey);
    }
    else
    {
        apiKey = File.ReadAllText("apikey.txt");
    }
    Console.WriteLine("API KEY: https://" + domain + "/" + apiKey);
    return apiKey;
}