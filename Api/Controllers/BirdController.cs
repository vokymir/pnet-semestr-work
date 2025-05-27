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

    public BirdController(AppDbContext context, ILogger<BirdController> logger)
    {
        _context = context;
        _logger = logger;
        Init();
    }

    [HttpPost("Create")]
    public IActionResult Add(BirdDto bd)
    {
        Bird b = bd.ToEntity();
        try
        {
            _birdRepo.Add(b);
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
        var bs = _birdRepo.GetAll();
        if (bs is null) return NotFound("No bird exists.");
        else
        {
            List<BirdDto> bds = new();
            foreach (var b in bs)
                bds.Add(b.ToFullDto());
            return Ok(bds);
        }
    }

    [HttpGet("GetByPrefix/{prefix}")]
    public IActionResult GetByPrefix(string prefix)
    {
        var bs = _birdRepo.GetByPrefix(prefix);
        if (bs is null) return NotFound("No bird with such prefix found...");

        List<BirdDto> bds = new();
        foreach (var b in bs)
            bds.Add(b.ToFullDto());

        return Ok(bds);
    }

    [HttpGet("GetById/{id}")]
    public IActionResult GetById(int id)
    {
        Bird? b = _birdRepo.GetById(id);
        if (b is null) return NotFound();

        BirdDto bd = b.ToFullDto();
        return Ok(bd);
    }

    [HttpPatch("AddToComment")]
    public IActionResult ProlongComment(int birdId, string comment)
    {
        Bird? b = _birdRepo.GetById(birdId);
        if (b is null) return NotFound();
        b.Comment += comment;
        try
        {
            _birdRepo.Update(b);
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }
        return Ok();
    }

    [HttpPatch("EditComment/{token}")]
    public IActionResult EditComment(string token, int birdId, string comment)
    {
        var adminResponse = AuthAdminByToken(token);
        if (!adminResponse.Equals(Ok()))
            return adminResponse;


        Bird? b = _birdRepo.GetById(birdId);
        if (b is null) return NotFound("Bird not found.");

        b.Comment = comment;

        try
        {
            _birdRepo.Update(b);
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }
        return Ok();
    }
}
