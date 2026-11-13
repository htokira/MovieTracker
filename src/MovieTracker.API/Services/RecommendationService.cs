using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace MovieTracker.API.Services
{
    public interface IRecommendationService
    {
        Task<RecommendationsResponse> GetRecommendationsAsync(Guid userId);
    }

    public class RecommendationService : IRecommendationService
    {
        public const int MaxResults = 20;

        private const int MaxSeedMovies = 5;
        private const int MaxSeedGenres = 3;
        private const int MaxPopularPages = 3;

        // Ваги скорингу
        private const double GenreWeight = 0.5;
        private const double SimilarityWeight = 0.3;
        private const double RatingWeight = 0.2;
        private const double DislikedGenrePenalty = 0.3;
        private const int SimilarityHitsCap = 3;

        private readonly MovieTrackerContext _db;
        private readonly ITmdbService _tmdbService;

        public RecommendationService(MovieTrackerContext db, ITmdbService tmdbService)
        {
            _db = db;
            _tmdbService = tmdbService;
        }

        private class Candidate
        {
            public MovieListItemDto Movie { get; init; } = null!;
            public int SimilarHits { get; set; }
            public double Score { get; set; }
        }

        public async Task<RecommendationsResponse> GetRecommendationsAsync(Guid userId)
        {
            var excludedIds = await GetExcludedMovieIdsAsync(userId);

            var profile = await _db.TasteProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile != null)
            {
                var personalized = await GetPersonalizedAsync(profile, excludedIds);

                if (personalized.Count > 0)
                {
                    // якщо кандидатів замало - дозаповнюємо популярними
                    if (personalized.Count < MaxResults)
                    {
                        excludedIds.UnionWith(personalized.Select(m => m.Id));
                        var topUp = await GetPopularAsync(excludedIds, MaxResults - personalized.Count);
                        personalized.AddRange(topUp);
                    }

                    return new RecommendationsResponse(true, personalized);
                }
            }

            // Fallback, бо профілю немає або в ньому недостатньо даних
            var popular = await GetPopularAsync(excludedIds, MaxResults);
            return new RecommendationsResponse(false, popular);
        }

        private async Task<HashSet<int>> GetExcludedMovieIdsAsync(Guid userId)
        {
            var reviewedIds = await _db.Reviews
                .Where(r => r.UserId == userId)
                .Select(r => r.MovieId)
                .ToListAsync();

            var trackedIds = await _db.UserMovieStatuses
                .Where(s => s.UserId == userId)
                .Select(s => s.MovieId)
                .ToListAsync();

            return reviewedIds.Concat(trackedIds).ToHashSet();
        }

        private async Task<List<MovieListItemDto>> GetPersonalizedAsync(
            TasteProfile profile, HashSet<int> excludedIds)
        {
            var preferences = Deserialize<List<GenrePreferenceDto>>(profile.GenrePreferencesJson);
            var dislikedGenreIds = Deserialize<List<int>>(profile.DislikedGenresIdsJson).ToHashSet();

            var seedMovieIds = Deserialize<List<int>>(profile.HighlyRatedMovieIdsJson)
                .Take(MaxSeedMovies)
                .ToList();

            var seedGenreIds = Deserialize<List<int>>(profile.FavoriteGenreIdsJson)
                .Take(MaxSeedGenres)
                .ToList();

            if (seedMovieIds.Count == 0 && seedGenreIds.Count == 0)
                return new List<MovieListItemDto>();

            var similarTasks = seedMovieIds
                .Select(id => _tmdbService.GetSimilarMoviesAsync(id))
                .ToList();

            var discoverTasks = seedGenreIds
                .Select(genreId => _tmdbService.GetDiscoverMoviesAsync(1, genreId))
                .ToList();

            var similarResults = await Task.WhenAll(similarTasks);
            var discoverResults = await Task.WhenAll(discoverTasks);

            var candidates = new Dictionary<int, Candidate>();

            foreach (var movie in similarResults.SelectMany(r => r.Items))
            {
                var candidate = GetOrAddCandidate(candidates, movie, excludedIds);
                if (candidate != null)
                    candidate.SimilarHits++;
            }

            foreach (var movie in discoverResults.SelectMany(r => r.Items))
                GetOrAddCandidate(candidates, movie, excludedIds);

            var genreScores = preferences.ToDictionary(p => p.GenreId, p => p.PreferenceScore);

            foreach (var candidate in candidates.Values)
                candidate.Score = CalculateScore(candidate, genreScores, dislikedGenreIds);

            return candidates.Values
                .OrderByDescending(c => c.Score)
                .ThenByDescending(c => c.Movie.Rating)
                .ThenBy(c => c.Movie.Id)
                .Take(MaxResults)
                .Select(c => c.Movie)
                .ToList();
        }

        private static Candidate? GetOrAddCandidate(
            Dictionary<int, Candidate> candidates, MovieListItemDto movie, HashSet<int> excludedIds)
        {
            if (excludedIds.Contains(movie.Id))
                return null;

            if (!candidates.TryGetValue(movie.Id, out var candidate))
            {
                candidate = new Candidate { Movie = movie };
                candidates[movie.Id] = candidate;
            }

            return candidate;
        }

        private static double CalculateScore(
            Candidate candidate,
            Dictionary<int, double> genreScores,
            HashSet<int> dislikedGenreIds)
        {
            var genres = candidate.Movie.Genres;

            // сер. подобання жанрів фільму з профілю [-1; 1]
            var genreScore = genres.Count == 0
                ? 0
                : genres.Average(g => genreScores.GetValueOrDefault(g.Id, 0));

            // у скількох улюблених фільмів кандидат трапився серед схожих: [0; 1]
            var similarity = Math.Min(candidate.SimilarHits, SimilarityHitsCap) / (double)SimilarityHitsCap;

            // Рейтинг TMDB [0; 1]
            var rating = Math.Clamp(candidate.Movie.Rating / 10.0, 0, 1);

            var penalty = genres.Any(g => dislikedGenreIds.Contains(g.Id))
                ? DislikedGenrePenalty
                : 0;

            return GenreWeight * genreScore + SimilarityWeight * similarity + RatingWeight * rating - penalty;
        }

        private async Task<List<MovieListItemDto>> GetPopularAsync(HashSet<int> excludedIds, int count)
        {
            var result = new List<MovieListItemDto>();
            var seen = new HashSet<int>(excludedIds);

            for (var page = 1; page <= MaxPopularPages && result.Count < count; page++)
            {
                var pagedResult = await _tmdbService.GetPopularMoviesAsync(page);
                var movies = pagedResult.Items.ToList();
                if (movies.Count == 0)
                    break;

                foreach (var movie in movies)
                {
                    if (result.Count >= count)
                        break;

                    if (seen.Add(movie.Id))
                        result.Add(movie);
                }
            }

            return result;
        }

        private static T Deserialize<T>(string json) where T : new()
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json) ?? new T();
            }
            catch (JsonException)
            {
                return new T();
            }
        }
    }
}
