using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.Data;

namespace MovieTracker.API.Services;

public class TasteProfileService
{
    private readonly MovieTrackerContext _db;

    private const double HalfLifeDays = 180.0;
    private static double Lambda = Math.Log(2) / HalfLifeDays;
    private const double PriorStrength = 3.0;
    private const double ConfidenceScale = 10.0;

    public TasteProfileService(MovieTrackerContext db)
    {
        _db = db;
    }

    private sealed class WeightedReview
    {
        public double Score { get; set; }
        public double Weight { get; set; }
        public bool IsReview { get; set; }
        public List<(int Id, string Name)> Genres { get; set; } = new();
    }

    private sealed class GenreCalculation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public double WeightedScoreSum { get; set; }
        public double WeightSum { get; set; }
        public double WeightSquaredSum { get; set; }

        public double PreferenceScore { get; set; }
        public double Variance { get; set; }
        public double Confidence { get; set; }

        public int ReviewsCount { get; set; }

        public List<(double Score, double Weight)> Observations { get; set; } = new();
    }

    public async Task<TasteProfileDto> AnalyzeAsync(Guid userId)
    {
        var now = DateTime.UtcNow;

        var reviews = await _db.Reviews
            .AsNoTracking()
            .Include(r => r.Movie)
                .ThenInclude(m => m.Genres)
            .Where(r => r.UserId == userId)
            .ToListAsync();

        var statuses = await _db.UserMovieStatuses
            .AsNoTracking()
            .Include(s => s.Movie)
                .ThenInclude(m => m.Genres)
            .Where(s => s.UserId == userId)
            .ToListAsync();

        var collectionMovies = await _db.CollectionMovies
            .AsNoTracking()
            .Include(cm => cm.Collection)
            .Include(cm => cm.Movie)
                .ThenInclude(m => m.Genres)
            .Where(cm => cm.Collection.UserId == userId)
            .ToListAsync();

        var weightedReviews = new List<WeightedReview>();

        var reviewedMovieIds = reviews
            .Select(r => r.MovieId)
            .ToHashSet();

        foreach (var review in reviews)
        {
            var ageDays = Math.Max(
                0, (now - review.UpdatedAt).TotalDays);

            var timeWeight = Math.Exp(-Lambda * ageDays);

            var normalizedScore = (review.Rating - 5.5) / 4.5;

            weightedReviews.Add(new WeightedReview
            {
                Score = normalizedScore,
                Weight = timeWeight,
                IsReview = true,
                Genres = review.Movie.Genres
                    .Select(g => (g.Id, g.Name))
                    .ToList()
            });
        }

        foreach (var status in statuses)
        {
            if (reviewedMovieIds.Contains(status.MovieId))
                continue;

            var (score, baseWeight) = status.Status switch
            {
                WatchStatus.Watched => (0.15, 0.5),
                WatchStatus.Planned => (0.10, 0.3),
                WatchStatus.Dropped => (-0.15, 0.5),
                _ => (0.0, 0.0)
            };

            if (baseWeight == 0)
                continue;

            var ageDays = Math.Max(
                0, (now - status.UpdatedAt).TotalDays);

            var timeWeight = Math.Exp(-Lambda * ageDays);

            weightedReviews.Add(new WeightedReview
            {
                Score = score,
                Weight = baseWeight * timeWeight,
                IsReview = false,
                Genres = status.Movie.Genres
                    .Select(g => (g.Id, g.Name))
                    .ToList()
            });
        }

        var collectedMovies = collectionMovies
            .GroupBy(cm => cm.MovieId)
            .Select(g => g.OrderByDescending(cm => cm.AddedAt).First());

        foreach (var collectionMovie in collectedMovies)
        {
            var ageDays = Math.Max(
                0, (now - collectionMovie.AddedAt).TotalDays);

            var timeWeight = Math.Exp(-Lambda * ageDays);

            weightedReviews.Add(new WeightedReview
            {
                Score = 0.25,
                Weight = 0.2 * timeWeight,
                IsReview = false,
                Genres = collectionMovie.Movie.Genres
                    .Select(g => (g.Id, g.Name))
                    .ToList()
            });
        }

        var genreCalculations = new Dictionary<int, GenreCalculation>();

        foreach (var review in weightedReviews)
        {
            if (review.Genres.Count == 0)
                continue;

            var genreWeight = review.Weight / review.Genres.Count;

            foreach (var genre in review.Genres)
            {
                if (!genreCalculations.TryGetValue(genre.Id, out var calculation))
                {
                    calculation = new GenreCalculation
                    {
                        Id = genre.Id,
                        Name = genre.Name
                    };

                    genreCalculations[genre.Id] = calculation;
                }

                calculation.WeightedScoreSum +=
                    review.Score * genreWeight;

                calculation.WeightSum += genreWeight;

                calculation.WeightSquaredSum +=
                    genreWeight * genreWeight;

                if (review.IsReview)
                    calculation.ReviewsCount++;

                calculation.Observations.Add(
                    (review.Score, genreWeight));
            }
        }

        var preferences = new List<GenrePreferenceDto>();

        foreach (var genre in genreCalculations.Values)
        {
            var denominator = genre.WeightSum + PriorStrength;

            // Байєсівське згладжування
            genre.PreferenceScore =
                genre.WeightedScoreSum / denominator;

            // Зважена ефективна кількість оцінок
            var effectiveCount = genre.WeightSquaredSum > 0
                ? genre.WeightSum * genre.WeightSum /
                  genre.WeightSquaredSum
                : 0;

            genre.Confidence =
                1 - Math.Exp(-effectiveCount / ConfidenceScale);

            // Зважена дисперсія
            if (genre.WeightSum > 0)
            {
                genre.Variance = genre.Observations.Sum(o =>
                    o.Weight * Math.Pow(
                        o.Score - genre.PreferenceScore, 2))
                    / genre.WeightSum;
            }

            preferences.Add(new GenrePreferenceDto
            {
                GenreId = genre.Id,
                GenreName = genre.Name,
                PreferenceScore = Math.Round(
                    genre.PreferenceScore, 4),
                ReviewsCount = genre.ReviewsCount,
                Confidence = Math.Round(
                    genre.Confidence, 4)
            });
        }

        preferences = preferences
            .OrderByDescending(p => p.PreferenceScore)
            .ToList();

        var favoriteGenresIds = preferences
            .Where(p => p.PreferenceScore > 0.05 &&
                        p.Confidence >= 0.1)
            .Take(5)
            .Select(p => p.GenreId)
            .ToList();

        var highlyRatedMovieIds = reviews
            .Where(r => r.Rating >= 8)
            .Select(r => r.MovieId)
            .Distinct()
            .ToList();

        var dislikedGenresIds = preferences
            .Where(p => p.PreferenceScore < -0.05 &&
                        p.Confidence >= 0.1)
            .OrderBy(p => p.PreferenceScore)
            .Take(5)
            .Select(p => p.GenreId)
            .ToList();

        var genreWeights = genreCalculations.Values
            .Select(g => g.WeightSum)
            .Where(w => w > 0)
            .ToList();

        double diversity = 0;

        if (genreWeights.Count > 1)
        {
            var totalWeight = genreWeights.Sum();

            var entropy = genreWeights.Sum(w =>
            {
                var probability = w / totalWeight;
                return -probability * Math.Log(probability);
            });

            diversity = entropy / Math.Log(genreWeights.Count);
        }

        double consistency = 0;

        var allObservations = genreCalculations.Values
            .SelectMany(g => g.Observations)
            .ToList();

        if (allObservations.Count > 0)
        {
            var totalWeight = allObservations.Sum(o => o.Weight);

            var mean = allObservations.Sum(o =>
                o.Score * o.Weight) / totalWeight;

            var variance = allObservations.Sum(o =>
                o.Weight * Math.Pow(o.Score - mean, 2))
                / totalWeight;

            consistency = Math.Clamp(
                1 - variance / 1.0, 0, 1);
        }

        var observationWeights = weightedReviews
            .Select(r => r.Weight)
            .ToList();

        var sumWeights = observationWeights.Sum();
        var sumSquaredWeights = observationWeights.Sum(w => w * w);

        var effectiveObservations = sumSquaredWeights > 0
            ? sumWeights * sumWeights / sumSquaredWeights
            : 0;

        var confidence = 1 - Math.Exp(
            -effectiveObservations / ConfidenceScale);

        var result = new TasteProfileDto
        {
            ReviewsCount = reviews.Count,
            GenrePreferences = preferences,
            FavoriteGenreIds = favoriteGenresIds,
            HighlyRatedMovieIds = highlyRatedMovieIds,
            DislikedGenreIds = dislikedGenresIds,
            TasteDiversity = Math.Round(diversity, 4),
            TasteConsistency = Math.Round(consistency, 4),
            Confidence = Math.Round(confidence, 4),
            CalculatedAt = now
        };

        await SaveProfileAsync(userId, result);

        return result;
    }

    private async Task SaveProfileAsync(Guid userId, TasteProfileDto result)
    {
        var profile = await _db.Set<TasteProfile>()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            profile = new TasteProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId
            };

            _db.Set<TasteProfile>().Add(profile);
        }

        profile.ReviewsCount = result.ReviewsCount;

        profile.GenrePreferencesJson =
            JsonSerializer.Serialize(result.GenrePreferences);

        profile.FavoriteGenreIdsJson =
            JsonSerializer.Serialize(result.FavoriteGenreIds);

        profile.DislikedGenresIdsJson =
            JsonSerializer.Serialize(result.DislikedGenreIds);

        profile.HighlyRatedMovieIdsJson =
            JsonSerializer.Serialize(result.HighlyRatedMovieIds);

        profile.TasteDiversity = result.TasteDiversity;
        profile.TasteConsistency = result.TasteConsistency;
        profile.Confidence = result.Confidence;
        profile.CalculatedAt = result.CalculatedAt;

        await _db.SaveChangesAsync();
    }
}
