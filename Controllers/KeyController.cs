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
    private ILogger<KeyController> logger { get; set; }

    public KeyController(XrayManager xrayManager, ILogger<KeyController> logger)
    {
        this.xrayManager = xrayManager;
        this.logger = logger;
    }

    public ActionResult Index()
    {
        return Ok("Its worked");
    }

    [HttpPost("access-keys")]
    public IActionResult CreateKey()
    {
        try
        {
            var key = xrayManager.CreateKey();
            return Ok(key);
        } 
        catch (Exception ex)
        {
            logger.LogError(ex, "Error create key");
            return Problem("Error create key");
        }
    }

    [HttpDelete("access-keys")]
    public IActionResult DeleteKey([FromBody] XrayId xrayId)
    {
        try
        {
            var key = xrayManager.DeleteKey(xrayId.Id);
            return Ok(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error delete key");
            return Problem("Error delete key");
        }
    }

    [HttpGet("access-keys")]
    public IActionResult GetKeys()
    {
        try
        {
            var keys = xrayManager.GetKeys();
            return Ok(keys);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error get keys");
            return Problem("Error get keys");
        }
    }

    [HttpPut("access-keys")]
    public IActionResult HasKey([FromBody] XrayId xrayId)
    {
        try
        {
            var res = xrayManager.HasKey(xrayId.Id);
            return Ok(res);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error check key");
            return Problem("Error check key");
        }
    }
}
