namespace BirdWatching.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using BirdWatching.Shared.Model;

/// <summary>
/// Can do:
/// - Create, read (public)
/// - update comment (public)
/// -
/// Should do:
/// - update, delete (private)
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

    [HttpGet("GetById")]
    public IResult Get(int recordId)
    {
        var r = _recordRepo.GetById(recordId);
        if (r is null) return Results.NotFound();
        else return Results.Ok(r.ToFullDto());
    }

    [HttpGet("GetByWatcher")]
    public IResult GetWatchersRecords(int watcherId)
    {
        var records = _recordRepo.GetWatcherRecords(watcherId);

        List<RecordDto> rds = new();
        foreach (var r in records)
            rds.Add(r.ToFullDto());

        return Results.Ok(rds);
    }

    [HttpPatch("AddToComment")]
    public IResult ProlongComment(int recordId, string comment)
    {
        var r = _recordRepo.GetById(recordId);
        if (r is null) return Results.NotFound();
        r.Comment += comment;
        try
        {
            _recordRepo.Update(r);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }
        return Results.Ok();
    }

    [HttpPatch("EditComment/{token}")]
    public IResult EditComment(string token, int recordId, string comment)
    {
        var r = _recordRepo.GetById(recordId);
        if (r is null) return Results.NotFound("Record not found.");

        // if either is owner of record or is admin
        var response = AuthUserByToken(token);
        if (!response.Result.Equals(Results.Ok()))
        {
            var adminResponse = AuthAdminByToken(token);
            if (!adminResponse.Equals(Results.Ok()))
                return adminResponse;
        }
        else
        {
            if (!response.User!.Watchers.Contains(r.Watcher))
                return Results.Problem("Don't have permission to edit this comment.");
        }

        r.Comment = comment;

        try
        {
            _recordRepo.Update(r);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }
        return Results.Ok();
    }
}
