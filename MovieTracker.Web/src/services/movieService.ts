import api from "./api";
import type { MovieDetailDto, MovieListItemDto } from "../types/movie";

const MOVIES_URL = "/movies";

interface GetMoviesParams {
  query?: string;
  page?: number;
  genreId?: number;
  sortBy?: string; 
}

function ensureMovieList(data: unknown): MovieListItemDto[] {
  if (!Array.isArray(data)) {
    throw new Error("Бекенд повернув неочікувану відповідь.");
  }
  return data as MovieListItemDto[];
}

export const movieService = {
  getMovies: async (
    params: GetMoviesParams = {}
  ): Promise<MovieListItemDto[]> => {
    const response = await api.get<unknown>(MOVIES_URL, { params });
    return ensureMovieList(response.data);
  },

  getPopularMovies: async (page: number = 1): Promise<MovieListItemDto[]> => {
    const response = await api.get<unknown>(`${MOVIES_URL}/popular`, {
      params: { page },
    });
    return ensureMovieList(response.data);
  },

  getMovieById: async (id: number): Promise<MovieDetailDto> => {
    const response = await api.get<MovieDetailDto>(`${MOVIES_URL}/${id}`);
    return response.data;
  },
};