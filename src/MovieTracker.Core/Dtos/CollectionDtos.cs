using System.ComponentModel.DataAnnotations;

namespace MovieTracker.Core.Dtos
{
    public record CreateCollectionRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [StringLength(1000)] string? Description,
    bool IsPublic);

    public record UpdateCollectionRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [StringLength(1000)] string? Description,
    bool IsPublic);

    public record CollectionSummaryResponse(
        Guid Id, Guid UserId, string OwnerName, string Name, string? Description,
        bool IsPublic, int MoviesCount, DateTime CreatedAt, DateTime UpdatedAt);

    public record CollectionMovieResponse(
        int MovieId, string Title, string? PosterUrl, int? ReleaseYear, double Rating, DateTime AddedAt);

    public record CollectionDetailResponse(
        Guid Id, Guid UserId, string OwnerName, string Name, string? Description,
        bool IsPublic, DateTime CreatedAt, DateTime UpdatedAt,
        List<CollectionMovieResponse> Movies);
}