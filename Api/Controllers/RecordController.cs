namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Can do:
/// -
/// Should do:
/// - Create, read (public)
/// - update, delete (private)
/// - update comment (public)
/// </summary>
[ApiController]
public class RecordController : BaseApiController
{
    private readonly ILogger<RecordController> _logger;

    public RecordController(ILogger<RecordController> logger)
    {
        _logger = logger;
        Init();
    }

}
