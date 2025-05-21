namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Can do:
/// -
/// Should do:
/// - Create, Get (public)
/// - Update, delete (admin)
/// </summary>
[ApiController]
public class BirdController : BaseApiController
{
    private readonly ILogger<BirdController> _logger;

    public BirdController(ILogger<BirdController> logger)
    {
        _logger = logger;
        Init();
    }

}
