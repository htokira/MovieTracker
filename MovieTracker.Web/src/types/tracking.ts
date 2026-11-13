export const WatchStatus = {
  Watched: "Watched",
  Planned: "Planned",
  Dropped: "Dropped",
} as const;

export type WatchStatus = (typeof WatchStatus)[keyof typeof WatchStatus];

export const WATCH_STATUS_LABELS: Record<WatchStatus, string> = {
  [WatchStatus.Watched]: "Переглянуто",
  [WatchStatus.Planned]: "В планах",
  [WatchStatus.Dropped]: "Кинуто",
};

export interface WatchStatusResponse {
  movieId: number;
  status: WatchStatus;
  updatedAt: string;
}

export interface SetWatchStatusRequest {
  status: WatchStatus;
}

export interface WatchStatusItemResponse {
  movieId: number;
  title: string;
  posterUrl: string | null;
  releaseYear: number;
  status: WatchStatus;
  updatedAt: string;
}

export interface UserMoviesResponse {
  page: number;
  pageSize: number;
  totalCount: number;
  items: WatchStatusItemResponse[];
}