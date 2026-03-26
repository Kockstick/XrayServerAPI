using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XrayServerAPI.InstallXray;

namespace XrayServerAPI.Controllers;

public class KeyController : Controller
{
    public KeyController()
    {

    }

    public ActionResult Index()
    {
        return Ok("Its worked");
    }
}
