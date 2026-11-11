using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTracker.API.Services;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;

namespace MovieTracker.API.Controllers
{
    [ApiController]
    [Route("api/recommendations")]
    [Authorize]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationsController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        private bool TryGetUserId(out Guid userId)
        {
            return Guid.TryParse(User.FindFirst("sub")?.Value, out userId);
        }

        [HttpGet("me")]
        public async Task<ActionResult<RecommendationsResponse>> GetMyRecommendations()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var result = await _recommendationService.GetRecommendationsAsync(userId);
            return Ok(result);
        }
    }
}
