using System.ComponentModel;
using MovieTracker.Core.Dtos;

namespace MovieTracker.UI.Models;

public class TrackedMovieItem
{
    private const string TmdbImageBase = "https://image.tmdb.org/t/p/w185";

    public int MovieId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int? Year { get; init; }
    public string? PosterUrl { get; init; }
    public DateTime UpdatedAt { get; init; }

    public string YearText => Year?.ToString() ?? "—";
    public string UpdatedText => $"Оновлено {UpdatedAt.ToLocalTime():dd.MM.yyyy}";

    public static TrackedMovieItem From(WatchStatusItemResponse dto) => new()
    {
        MovieId = dto.MovieId,
        Title = dto.Title,
        Year = dto.ReleaseYear,
        PosterUrl = NormalizePoster(dto.PosterUrl),
        UpdatedAt = dto.UpdatedAt
    };

    // У БД може лежати і повний URL, і TMDB-шлях виду "/abc.jpg"
    internal static string? NormalizePoster(string? poster)
    {
        if (string.IsNullOrWhiteSpace(poster)) return null;
        if (poster.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return poster;
        return TmdbImageBase + (poster.StartsWith('/') ? poster : "/" + poster);
    }
}



public class CollectionListItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsPublic { get; init; }
    public int MoviesCount { get; init; }

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
    public string InfoText => $"{(IsPublic ? "Публічна" : "Приватна")}  ·  Фільмів: {MoviesCount}";

    public static CollectionListItem From(CollectionSummaryResponse dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        IsPublic = dto.IsPublic,
        MoviesCount = dto.MoviesCount
    };
}

public class CollectionMovieItem
{
    public int MovieId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int? Year { get; init; }
    public string? PosterUrl { get; init; }
    public double Rating { get; init; }

    public string InfoText =>
        (Year?.ToString() ?? "—") + (Rating > 0 ? $"  ·  ★ {Rating:F1}" : string.Empty);

    public static CollectionMovieItem From(CollectionMovieResponse dto) => new()
    {
        MovieId = dto.MovieId,
        Title = dto.Title,
        Year = dto.ReleaseYear,
        PosterUrl = TrackedMovieItem.NormalizePoster(dto.PosterUrl),
        Rating = dto.Rating
    };
}

public class CollectionDetail
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsPublic { get; init; }
    public List<CollectionMovieItem> Movies { get; init; } = new();

    public static CollectionDetail From(CollectionDetailResponse dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        IsPublic = dto.IsPublic,
        Movies = dto.Movies.Select(CollectionMovieItem.From).ToList()
    };
}
