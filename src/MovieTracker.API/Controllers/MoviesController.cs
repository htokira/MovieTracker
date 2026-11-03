using Microsoft.AspNetCore.Mvc;
using MovieTracker.API.Services;
using MovieTracker.Core.Dtos;

namespace MovieTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly ITmdbService _tmdbService;

    public MoviesController(ITmdbService tmdbService)
    {
        _tmdbService = tmdbService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMovies([FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int? genreId = null, [FromQuery] string? sortBy = null)
    {
        var movies = string.IsNullOrWhiteSpace(query)
            ? await _tmdbService.GetDiscoverMoviesAsync(page, genreId, sortBy)
            : await _tmdbService.SearchMoviesAsync(query, page, genreId);

        return Ok(movies);
    }

    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularMovies([FromQuery] int page = 1)
    {
        var movies = await _tmdbService.GetPopularMoviesAsync(page);
        return Ok(movies);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovieDetailDto>> GetMovieById(int id)
    {
        var movie = await _tmdbService.GetMovieDetailsAsync(id);
        if (movie == null) return NotFound();
        return Ok(movie);
    }
}