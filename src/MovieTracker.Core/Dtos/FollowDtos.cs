namespace MovieTracker.Core.Dtos
{
    public record FollowUserResponse(
        Guid Id, string DisplayName, string? AvatarUrl, DateTime FollowedAt);

    public record FollowStatsResponse(
        int FollowersCount, int FollowingCount, bool IsFollowedByMe);
}