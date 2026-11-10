namespace MovieTracker.Core.Dtos;

public class GenrePreferenceDto
{
    public int GenreId { get; set; }
    public string GenreName { get; set; } = string.Empty;

    public double PreferenceScore { get; set; }

    public int ReviewsCount { get; set; }

    public double Confidence { get; set; }
}

public class TasteProfileDto
{
    public int ReviewsCount { get; set; }

    public List<GenrePreferenceDto> GenrePreferences { get; set; } = new();

    public List<string> FavoriteGenres { get; set; } = new();
    public List<string> DislikedGenres { get; set; } = new();

    public double TasteDiversity { get; set; }
    public double TasteConsistency { get; set; }
    public double Confidence { get; set; }

    public DateTime CalculatedAt { get; set; }
}