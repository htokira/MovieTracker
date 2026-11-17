using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieTracker.API.Services;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.Data;

namespace MovieTracker.API.Controllers;

[ApiController]
[Route("api/taste-profile")]
[Authorize]
public class TasteProfileController : ControllerBase
{
    private readonly TasteProfileService _service;
    private readonly MovieTrackerContext _db;

    public TasteProfileController(TasteProfileService service, MovieTrackerContext db)
    {
        _service = service;
        _db = db;
    }

    private bool TryGetUserId(out Guid userId)
    {
        return Guid.TryParse(
            User.FindFirst("sub")?.Value,
            out userId);
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<TasteProfileDto>> Analyze()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var profile = await _service.AnalyzeAsync(userId);

        return Ok(profile);
    }

    [HttpGet("me")]
    public async Task<ActionResult<TasteProfileDto>> GetMyProfile()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var profile = await _db.Set<TasteProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            return NotFound(new
            {
                message = "Смаковий профіль ще не створено. Виконайте аналіз."
            });

        return Ok(MapToDto(profile));
    }

    [HttpGet("{userId:guid}")]
    [HttpGet("/api/users/{userId:guid}/taste-profile")]
    [AllowAnonymous]
    public async Task<ActionResult<TasteProfileDto>> GetUserProfile(Guid userId)
    {
        if (!await _db.Users.AnyAsync(u => u.Id == userId))
            return NotFound();

        var profile = await _db.Set<TasteProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            return NotFound(new
            {
                message = "Смаковий профіль цього користувача ще не створено."
            });

        return Ok(MapToDto(profile));
    }

    private static TasteProfileDto MapToDto(TasteProfile profile) => new()
    {
        ReviewsCount = profile.ReviewsCount,
        GenrePreferences = JsonSerializer.Deserialize<List<GenrePreferenceDto>>(profile.GenrePreferencesJson) ?? new(),
        FavoriteGenreIds = JsonSerializer.Deserialize<List<int>>(profile.FavoriteGenreIdsJson) ?? new(),
        DislikedGenreIds = JsonSerializer.Deserialize<List<int>>(profile.DislikedGenresIdsJson) ?? new(),
        HighlyRatedMovieIds = JsonSerializer.Deserialize<List<int>>(profile.HighlyRatedMovieIdsJson) ?? new(),
        TasteDiversity = profile.TasteDiversity,
        TasteConsistency = profile.TasteConsistency,
        Confidence = profile.Confidence,
        CalculatedAt = profile.CalculatedAt
    };
}