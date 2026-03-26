using Microsoft.AspNetCore.Mvc;
using XrayServerAPI.InstallXray;
using System.IO;

namespace XrayServerAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController : Controller
{
    public HomeController()
    {
        if (!System.IO.File.Exists("installed.flag"))
        {
            var domain = "nl3.divpn.ru";
            new InstallXrayManager(domain).Install();
            System.IO.File.WriteAllText("installed.flag", "ok");
        }
    }
}
