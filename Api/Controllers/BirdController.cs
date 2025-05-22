namespace BirdWatching.Api.Controllers;

using BirdWatching.Shared.Model;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Can do:
/// - Create, Get (public)
/// - add to comment (public), edit comment (admin)
/// Should do:
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

    [HttpPost("Create")]
    public IResult Add(BirdDto bd)
    {
        Bird b = bd.ToEntity();
        try
        {
            _birdRepo.Add(b);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }

        return Results.Ok();
    }

    [HttpGet("GetAll")]
    public IResult GetAll()
    {
        var bs = _birdRepo.GetAll();
        if (bs is null) return Results.NotFound("No bird exists.");
        else
        {
            List<BirdDto> bds = new();
            foreach (var b in bs)
                bds.Add(b.ToFullDto());
            return Results.Ok(bds);
        }
    }

    [HttpGet("GetByPrefix/{prefix}")]
    public IResult GetByPrefix(string prefix)
    {
        var bs = _birdRepo.GetByPrefix(prefix);
        if (bs is null) return Results.NotFound("No bird with such prefix found...");

        List<BirdDto> bds = new();
        foreach (var b in bs)
            bds.Add(b.ToFullDto());

        return Results.Ok(bds);
    }

    [HttpGet("GetById")]
    public IResult GetById(int id)
    {
        Bird? b = _birdRepo.GetById(id);
        if (b is null) return Results.NotFound();

        BirdDto bd = b.ToFullDto();
        return Results.Ok(bd);
    }

    [HttpPatch("AddToComment")]
    public IResult ProlongComment(int birdId, string comment)
    {
        Bird? b = _birdRepo.GetById(birdId);
        if (b is null) return Results.NotFound();
        b.Comment += comment;
        try
        {
            _birdRepo.Update(b);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }
        return Results.Ok();
    }

    [HttpPatch("EditComment/{token}")]
    public IResult EditComment(string token, int birdId, string comment)
    {
        var adminResponse = AuthAdminByToken(token);
        if (!adminResponse.Equals(Results.Ok()))
            return adminResponse;


        Bird? b = _birdRepo.GetById(birdId);
        if (b is null) return Results.NotFound("Bird not found.");

        b.Comment = comment;

        try
        {
            _birdRepo.Update(b);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }
        return Results.Ok();
    }
}
