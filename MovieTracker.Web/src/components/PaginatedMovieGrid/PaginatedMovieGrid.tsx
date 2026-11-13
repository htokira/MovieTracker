import Button from "@/components/Button/Button";
import ErrorMessage from "@/components/ErrorMessage/ErrorMessage";
import MovieGrid from "@/components/MovieGrid/MovieGrid";
import type { MovieListItemDto } from "@/types/movie";
import styles from "./paginatedMovieGrid.module.css";

interface PaginatedMovieGridProps {
  movies: MovieListItemDto[];
  hasMore: boolean;
  isLoadingMore: boolean;
  onLoadMore: () => void;
  loadMoreError: string | null;
  emptyMessage: string;
}

function PaginatedMovieGrid({
  movies,
  hasMore,
  isLoadingMore,
  onLoadMore,
  loadMoreError,
  emptyMessage,
}: PaginatedMovieGridProps) {
  if (movies.length === 0) {
    return <p className={styles.empty}>{emptyMessage}</p>;
  }

  return (
    <>
      <MovieGrid movies={movies} />

      {loadMoreError && <ErrorMessage message={loadMoreError} />}

      {hasMore && (
        <div className={styles.more}>
          <Button type="button" variant="secondary" isLoading={isLoadingMore} onClick={onLoadMore}>
            Показати ще
          </Button>
        </div>
      )}
    </>
  );
}

export default PaginatedMovieGrid;