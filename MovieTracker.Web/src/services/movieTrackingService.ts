import { isAxiosError } from "axios";
import api from "@/services/api";
import type { MovieListItemDto } from "@/types/movie";
import type { SetWatchStatusRequest, UserMoviesResponse, WatchStatus, WatchStatusItemResponse, WatchStatusResponse } from "@/types/tracking";

export const MY_MOVIES_PAGE_SIZE = 20;
export function mapStatusItemToMovie(item: WatchStatusItemResponse): MovieListItemDto {
  return {
    id: item.movieId,
    title: item.title,
    posterUrl: item.posterUrl,
    releaseYear: item.releaseYear,
    description: "",
    rating: 0,
    mediaType: "movie",
    genres: [],
  };
}

export interface MyMoviesResult {
  movies: MovieListItemDto[];
  total: number;
}

export const movieTrackingService = {  
  async getStatus(movieId: number): Promise<WatchStatusResponse | null> {
    try {
      const response = await api.get<WatchStatusResponse>(`/movies/${movieId}/status`);
      return response.data;
    } catch (err) {
      if (isAxiosError(err) && err.response?.status === 404) {
        return null;
      }
      throw err;
    }
  },

  async setStatus(movieId: number, body: SetWatchStatusRequest): Promise<WatchStatusResponse> {
    const response = await api.put<WatchStatusResponse>(`/movies/${movieId}/status`, body);
    return response.data;
  },

  async removeStatus(movieId: number): Promise<void> {
    await api.delete(`/movies/${movieId}/status`);
  },

  async getMyMovies(status: WatchStatus | undefined, page: number): Promise<MyMoviesResult> {
    const response = await api.get<UserMoviesResponse>("/users/me/movies", {
      params: { status, page, pageSize: MY_MOVIES_PAGE_SIZE },
    });
    return {
      movies: response.data.items.map(mapStatusItemToMovie),
      total: response.data.totalCount,
    };
  },
};