using MovieTracker.Core.Models;

namespace MovieTracker.Core.Dtos;

public record SetWatchStatusRequest(WatchStatus Status);

public record WatchStatusResponse(int MovieId, WatchStatus Status, DateTime UpdatedAt);

public record WatchStatusItemResponse(
    int MovieId, string Title, string? PosterUrl, int ReleaseYear,
    WatchStatus Status, DateTime UpdatedAt);

public record UserMoviesResponse(
    int Page, int PageSize, int TotalCount, IReadOnlyList<WatchStatusItemResponse> Items);