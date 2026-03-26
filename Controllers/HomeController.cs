using Microsoft.AspNetCore.Mvc;
using XrayServerAPI.InstallXray;
using System.IO;

namespace XrayServerAPI.Controllers;

public class HomeController : Controller
{
    [HttpGet("install")]
    public IActionResult Install()
    {
        if (!System.IO.File.Exists("installed.flag"))
        {
            var domain = "nl3.divpn.ru";
            new InstallXrayManager(domain).Install();
            System.IO.File.WriteAllText("installed.flag", "ok");
        }
        return Ok("Installed");
    }
}
