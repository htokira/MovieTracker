using Microsoft.AspNetCore.Mvc;
using MovieTracker.API.Services;
using MovieTracker.Core.Dtos;
using MovieTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace MovieTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly ITmdbService _tmdbService;
    private readonly MovieTrackerContext _db;

    public MoviesController(ITmdbService tmdbService, MovieTrackerContext db)
    {
        _tmdbService = tmdbService;
        _db = db;
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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovieDetailDto>> GetMovieById(int id)
    {
        var localMovie = await _db.Movies
            .AsNoTracking()
            .Include(m => m.Genres)
            .Include(m => m.MoviePersons)
                .ThenInclude(mp => mp.Person)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (localMovie != null)
        {
            var movieDto = new MovieDetailDto
            {
                Id = localMovie.Id,
                Title = localMovie.Title,
                Description = localMovie.Description,
                ReleaseYear = localMovie.ReleaseYear,
                Rating = localMovie.Rating,
                MediaType = localMovie.MediaType,
                PosterUrl = localMovie.PosterUrl,

                Genres = localMovie.Genres
                    .Select(g => new GenreDto
                    {
                        Id = g.Id,
                        Name = g.Name
                    })
                    .ToList(),

                MoviePersons = localMovie.MoviePersons
                    .Select(mp => new MoviePersonDto
                    {
                        PersonId = mp.PersonId,
                        FullName = mp.Person.FullName,
                        PhotoUrl = mp.Person.PhotoUrl,
                        Role = mp.Role
                    })
                    .ToList()
            };

            return Ok(movieDto);
        }

        var tmdbMovie = await _tmdbService.GetMovieDetailsAsync(id);

        if (tmdbMovie == null)
            return NotFound();

        return Ok(tmdbMovie);
    }
}