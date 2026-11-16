using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.Data;
using MovieTracker.API.Services;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace MovieTracker.API.Controllers
{
    [ApiController]
    [Route("api/movies/{movieId:int:min(1)}/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly MovieTrackerContext _db;
        private readonly IMovieSyncService _movieSync;

        public ReviewsController(MovieTrackerContext db, IMovieSyncService movieSync)
        {
            _db = db;
            _movieSync = movieSync;
        }

        private static readonly Expression<Func<Review, ReviewResponse>> ToResponse = r =>
            new ReviewResponse(r.Id, r.MovieId, r.UserId, r.User.DisplayName, r.Rating, r.Text, r.CreatedAt, r.UpdatedAt);

        [HttpGet]
        public async Task<ActionResult<MovieReviewsResponse>> GetReviews(
            int movieId,
            [FromQuery, Range(1, 10000)] int page = 1,
            [FromQuery, Range(1, 50)] int pageSize = 20)
        {
            var query = _db.Reviews.AsNoTracking().Where(r => r.MovieId == movieId);

            var count = await query.CountAsync();
            var average = await query.AverageAsync(r => (double?)r.Rating);

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .ThenBy(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToResponse)
                .ToListAsync();

            return Ok(new MovieReviewsResponse(
                movieId,
                average is null ? null : Math.Round(average.Value, 1),
                count,
                page,
                pageSize,
                items));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ReviewResponse>> GetMyReview(int movieId)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var review = await _db.Reviews
                .AsNoTracking()
                .Where(r => r.UserId == userId && r.MovieId == movieId)
                .Select(ToResponse)
                .FirstOrDefaultAsync();

            return review is null ? NotFound() : Ok(review);
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<ActionResult<ReviewResponse>> UpsertMyReview(int movieId, UpsertReviewRequest request)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            if (!await _db.Users.AnyAsync(u => u.Id == userId)) return Unauthorized();

            bool movieExists;
            try
            {
                movieExists = await _movieSync.EnsureMovieAsync(movieId);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "TMDB is temporarily unavailable. Please try again later." });
            }
            catch (DbUpdateException)
            {
                _db.ChangeTracker.Clear();
                movieExists = await _db.Movies.AnyAsync(m => m.Id == movieId);
                if (!movieExists) throw;
            }

            if (!movieExists)
                return NotFound(new { message = $"Movie with ID {movieId} not found on TMDB." });

            var now = DateTime.UtcNow;
            var text = string.IsNullOrWhiteSpace(request.Text) ? null : request.Text.Trim();

            var review = await _db.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId);

            if (review is null)
            {
                review = new Review
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    MovieId = movieId,
                    CreatedAt = now
                };
                _db.Reviews.Add(review);
            }

            review.Rating = request.Rating;
            review.Text = text;
            review.UpdatedAt = now;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Review was changed by a concurrent request. Please retry." });
            }

            var response = await _db.Reviews
                .AsNoTracking()
                .Where(r => r.Id == review.Id)
                .Select(ToResponse)
                .SingleAsync();

            return Ok(response);
        }

        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteMyReview(int movieId)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var review = await _db.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId);
            if (review is null) return NotFound();

            _db.Reviews.Remove(review);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private bool TryGetUserId(out Guid userId) =>
            Guid.TryParse(User.FindFirst("sub")?.Value, out userId);
    }
}
