import { isAxiosError } from "axios";
import api from "./api";
import type { MovieReviewsResponse, ReviewResponse, UpsertReviewRequest } from "@/types/review";

function getReviewsUrl(movieId: number): string {
  return `/movies/${movieId}/reviews`;
}

export const reviewService = {
  // Відгуки фільму зі сер. оцінкою
  getReviews: async (
    movieId: number,
    page: number = 1,
    pageSize: number = 20
  ): Promise<MovieReviewsResponse> => {
    const response = await api.get<MovieReviewsResponse>(
      getReviewsUrl(movieId),
      { params: { page, pageSize } }
    );
    return response.data;
  },

  // Відгук поточного користувача
  getMyReview: async (movieId: number): Promise<ReviewResponse | null> => {
    try {
      const response = await api.get<ReviewResponse>(
        `${getReviewsUrl(movieId)}/me`
      );
      return response.data;
    } catch (error) {
      if (isAxiosError(error) && error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  },

  // Створити відгук або оновити 
  saveMyReview: async (
    movieId: number,
    data: UpsertReviewRequest
  ): Promise<ReviewResponse> => {
    const response = await api.put<ReviewResponse>(
      `${getReviewsUrl(movieId)}/me`,
      data
    );
    return response.data;
  },

  // Видалити свій відгук
  deleteMyReview: async (movieId: number): Promise<void> => {
    await api.delete(`${getReviewsUrl(movieId)}/me`);
  },
};