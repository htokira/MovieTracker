import api from "./api";

export interface FeedItemDto {
  type: string; // "review" або "status"
  userId: string;
  displayName: string;
  avatarUrl?: string | null;
  movieId: number;
  movieTitle: string;
  posterUrl?: string | null;
  rating?: number | null;
  reviewText?: string | null;
  status?: string | number | null;
  createdAt: string;
}

export interface FeedResponse {
  page: number;
  totalPages: number;
  totalResults: number;
  items: FeedItemDto[];
}

export const feedService = {
  getFeed: async (page: number = 1, pageSize: number = 20): Promise<FeedResponse> => {
    const response = await api.get<FeedResponse>("/users/me/feed", {
      params: { page, pageSize },
    });
    return response.data;
  },
};