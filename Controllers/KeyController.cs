using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XrayServerAPI.InstallXray;
using XrayServerAPI.Xray;

namespace XrayServerAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class KeyController : Controller
{
    private XrayManager xrayManager { get; set; }

    public KeyController(XrayManager xrayManager)
    {
        this.xrayManager = xrayManager;
    }

    public ActionResult Index()
    {
        return Ok("Its worked");
    }

    [HttpPost]
    [ActionName("access-keys")]
    public IActionResult CreateKey()
    {
        try
        {
            var key = xrayManager.CreateKey();
            return Ok(key);
        } 
        catch (Exception ex)
        {
            return Problem("Error create key");
        }
    }

    [HttpDelete]
    [ActionName("access-keys")]
    public IActionResult DeleteKey([FromBody] XrayId xrayId)
    {
        try
        {
            var key = xrayManager.DeleteKey(xrayId.Id);
            return Ok(key);
        }
        catch (Exception ex)
        {
            return Problem("Error delete key");
        }
    }

    [HttpGet]
    [ActionName("access-keys")]
    public IActionResult GetKeys()
    {
        try
        {
            var keys = xrayManager.GetKeys();
            return Ok(keys);
        }
        catch (Exception ex)
        {
            return Problem("Error get keys");
        }
    }

    [HttpPut]
    [ActionName("access-keys")]
    public IActionResult HasKey([FromBody] XrayId xrayId)
    {
        try
        {
            var res = xrayManager.HasKey(xrayId.Id);
            return Ok(res);
        }
        catch (Exception ex)
        {
            return Problem("Error check key");
        }
    }
}
