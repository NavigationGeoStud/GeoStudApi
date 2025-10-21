using Microsoft.AspNetCore.Mvc;

namespace GeoStud.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet]
    public string Get()
    {
        return "Hello from GeoStud API!";
    }
}
