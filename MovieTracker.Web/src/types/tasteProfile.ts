export interface GenrePreferenceDto {
  genreId: number;
  genreName: string;
  preferenceScore: number;
  reviewsCount: number;
  confidence: number;
}

export interface TasteProfileDto {
  reviewsCount: number;
  genrePreferences: GenrePreferenceDto[];
  favoriteGenres: string[];
  dislikedGenres: string[];
  favoriteGenreIds: number[];
  dislikedGenreIds: number[];
  highlyRatedMovieIds: number[];
  tasteDiversity: number;
  tasteConsistency: number;
  confidence: number;
  calculatedAt: string;
}

export interface TasteProfileInterpretation {
  archetype: string;
  archetypeDescription: string;
  title: string;
  titleDescription: string;
  diversityLabel: string;
  diversityDescription: string;
}