using Microsoft.AspNetCore.Mvc;

namespace Blog.Services.UserManager.API.Controllers;


[Route("[controller]")]
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public void Get()
    {

    }
}
