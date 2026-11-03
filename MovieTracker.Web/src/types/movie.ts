export interface MovieListItemDto {
  id: number;
  title: string;
  description: string;
  releaseYear: string | null;
  rating: number;
  mediaType: string;
  posterUrl: number | null;
}

export interface MovieDetailDto extends MovieListItemDto{
  genres: GenreDto[];
  moviePersons: MovieDetailDto[];
}

export interface GenreDto {
  id: number;
  name: string;
}

export interface MoviePersonDto {
  fullName: string;
  PhotoUrl: string | null;
  role: string;
}