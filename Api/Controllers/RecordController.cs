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

    public RecordController(AppDbContext context, ILogger<RecordController> logger)
    {
        _context = context;
        _logger = logger;
        Init();
    }

    [HttpPost("Create/{token}")]
    public IActionResult Create(string token, RecordDto recordDto)
    {
        var response = AuthUserByToken(token);
        if (!response.Result.Equals(Ok())) return response.Result;

        var bird = _birdRepo.GetById(recordDto.BirdId);
        if (bird == null) return NotFound("Bird not found.");

        var w = _watcherRepo.GetById(recordDto.WatcherId);
        if (w == null) return NotFound("Watcher not found.");
        if (!w.Curators.Contains(response.User!)) return Problem("Don't have permission to edit watcher.");

        Record r = recordDto.ToEntity();
        r.Watcher = w;
        r.Bird = bird;
        try
        {
            _recordRepo.Add(r);
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }

        return Ok();
    }

    [HttpGet("GetAll")]
    public IActionResult GetAll()
    {
        var rs = _recordRepo.GetAll();
        if (rs is null) return NotFound();
        List<RecordDto> rds = new();
        foreach (var r in rs)
            rds.Add(r.ToFullDto());
        return Ok(rds);
    }

    [HttpGet("GetById/{recordId}")]
    public IActionResult Get(int recordId)
    {
        var r = _recordRepo.GetById(recordId);
        if (r is null) return NotFound();
        else return Ok(r.ToFullDto());
    }

    [HttpGet("GetByWatcher/{watcherId}")]
    public IActionResult GetWatchersRecords(int watcherId)
    {
        try
        {
            var records = _recordRepo.GetWatcherRecords(watcherId);
            List<RecordDto> rds = new();
            foreach (var r in records)
                rds.Add(r.ToFullDto());

            return Ok(rds);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpPatch("AddToComment")]
    public IActionResult ProlongComment(int recordId, string comment)
    {
        var r = _recordRepo.GetById(recordId);
        if (r is null) return NotFound();
        r.Comment += comment;
        try
        {
            _recordRepo.Update(r);
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }
        return Ok();
    }

    [HttpPatch("EditComment/{token}")]
    public IActionResult EditComment(string token, int recordId, string comment)
    {
        var r = _recordRepo.GetById(recordId);
        if (r is null) return NotFound("Record not found.");

        // if either is owner of record or is admin
        var response = AuthUserByToken(token);
        if (!response.Result.Equals(Ok()))
        {
            var adminResponse = AuthAdminByToken(token);
            if (!adminResponse.Equals(Ok()))
                return adminResponse;
        }
        else
        {
            if (!response.User!.Watchers.Contains(r.Watcher))
                return Problem("Don't have permission to edit this comment.");
        }

        r.Comment = comment;

        try
        {
            _recordRepo.Update(r);
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }
        return Ok();
    }
}
