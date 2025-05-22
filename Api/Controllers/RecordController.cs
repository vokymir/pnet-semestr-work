namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using BirdWatching.Shared.Model;

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

    [HttpPost("Create/{token}")]
    public IResult Create(string token, RecordDto recordDto)
    {
        var response = AuthUserByToken(token);
        if (!response.Result.Equals(Results.Ok())) return response.Result;

        Record r = recordDto.ToEntity();

        var w = _watcherRepo.GetById(r.WatcherId);
        if (w is null) return Results.NotFound("Watcher not found.");
        if (!w.Curators.Contains(response.User!)) return Results.Problem("Don't have permission to edit watcher.");

        try
        {
            _recordRepo.Add(r);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }

        return Results.Ok();
    }

    [HttpGet("Get")]
    public IResult Get(int recordId)
    {
        var r = _recordRepo.GetById(recordId);
        if (r is null) return Results.NotFound();
        else return Results.Ok(r.ToFullDto());
    }

    [HttpGet("GetWatchers")]
    public IResult GetWatchers(int watcherId)
    {
        throw new NotImplementedException();
    }
}
