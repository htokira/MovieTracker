using MovieTracker.Core.Models;

namespace MovieTracker.Core.Dtos
{
    public record FollowUserResponse(
        Guid Id, string DisplayName, string? AvatarUrl, DateTime FollowedAt);

    public record FollowStatsResponse(
        int FollowersCount, int FollowingCount, bool IsFollowedByMe);

    // Type: "review" або "status"
    public record FeedItemResponse(
        string Type,
        Guid UserId,
        string DisplayName,
        string? AvatarUrl,
        int MovieId,
        string MovieTitle,
        string? PosterUrl,
        int? Rating,
        string? ReviewText,
        WatchStatus? Status,
        DateTime CreatedAt);
}
