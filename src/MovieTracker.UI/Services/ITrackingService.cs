using MovieTracker.Core.Models;
using MovieTracker.UI.Models;

namespace MovieTracker.UI.Services;

public interface ITrackingService
{
    Task<(List<TrackedMovieItem>? Items, string? Error)> GetMyMoviesAsync(WatchStatus status);
    Task<string?> CreateCollectionAsync(string name, string? description, bool isPublic);

    Task<(WatchStatus? Status, string? Error)> GetMovieStatusAsync(int movieId);
    Task<string?> SetMovieStatusAsync(int movieId, WatchStatus status);
    Task<string?> RemoveMovieStatusAsync(int movieId);

    Task<(List<CollectionListItem>? Items, string? Error)> GetMyCollectionsAsync();
    Task<(CollectionDetail? Detail, string? Error)> GetCollectionAsync(Guid collectionId);
    Task<string?> AddMovieToCollectionAsync(Guid collectionId, int movieId);
    Task<string?> RemoveMovieFromCollectionAsync(Guid collectionId, int movieId);
    Task<string?> DeleteCollectionAsync(Guid collectionId);
    Task RecalculateTasteProfileAsync();
}