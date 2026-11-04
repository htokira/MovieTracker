export interface MovieListItemDto {
  id: number;
  title: string;
  description: string;
  releaseYear: number | null;
  rating: number;
  mediaType: string;
  posterUrl: string | null;
  genres: GenreDto[];
}

export interface MovieDetailDto extends MovieListItemDto{
  moviePersons: MoviePersonDto[];
}

export interface GenreDto {
  id: number;
  name: string;
}

export interface MoviePersonDto {
  fullName: string;
  photoUrl: string | null;
  role: string;
}