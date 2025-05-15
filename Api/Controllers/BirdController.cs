namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
public class BirdController : BaseApiController
{
    private readonly ILogger<BirdController> _logger;

    public BirdController(ILogger<BirdController> logger)
    {
        _logger = logger;
    }
}
