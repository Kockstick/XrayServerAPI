using Microsoft.Extensions.FileProviders;
using XrayServerAPI.Install;
using XrayServerAPI.InstallXray;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.WebHost.UseUrls("http://127.0.0.1:5000");

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

app.MapControllers();

app.MapGet("/", () => "OK");

var domain = "nl3.divpn.ru";

if (!File.Exists("installed.flag"))
{
    new InstallXrayManager(domain).Install();
    File.WriteAllText("installed.flag", "ok");
}

var caddyStarter = new CaddyStarter(domain);
caddyStarter.Start();

app.Run();