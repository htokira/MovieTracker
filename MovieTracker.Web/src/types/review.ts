export interface UpsertReviewRequest {
  rating: number;
  text: string | null;
}

export interface ReviewResponse {
  id: string;
  movieId: number;
  userId: string;
  authorName: string;
  rating: number;
  text: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface MovieReviewsResponse {
  movieId: number;
  averageRating: number | null;
  totalReviews: number;
  page: number
  pageSize: number;
  items: ReviewResponse[]
}