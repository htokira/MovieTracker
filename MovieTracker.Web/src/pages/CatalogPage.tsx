import { useEffect, useState } from "react";
import Button from "../components/Button";
import ErrorMessage from "../components/ErrorMessage";
import Spinner from "../components/Spinner";
import MovieCarousel from "../features/MovieCarousel";
import MovieGrid from "../features/MovieGrid";
import { movieService } from "../services/movieService";
import type { MovieListItemDto } from "../types/movie";
import styles from "../styles/CatalogPage.module.css";

const CATALOG_SORT = "vote_count.desc";

function CatalogPage() {
  const [topMovies, setTopMovies] = useState<MovieListItemDto[]>([]);
  const [movies, setMovies] = useState<MovieListItemDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [hasMore, setHasMore] = useState<boolean>(true);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isLoadingMore, setIsLoadingMore] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [loadMoreError, setLoadMoreError] = useState<string | null>(null);

  useEffect(() => {
    let isIgnored = false;

    async function loadMovies() {
      try {
        const [top, firstPage] = await Promise.all([
          movieService.getPopularMovies(),
          movieService.getMovies({ page: 1, sortBy: CATALOG_SORT }),
        ]);
        if (!isIgnored) {
          setTopMovies(top);
          setMovies(firstPage);
          setHasMore(firstPage.length > 0);
        }
      } catch {
        if (!isIgnored) {
          setError("Не вдалося завантажити фільми. Спробуйте пізніше.");
        }
      } finally {
        if (!isIgnored) {
          setIsLoading(false);
        }
      }
    }

    loadMovies();

    return () => {
      isIgnored = true;
    };
  }, []);

  async function handleShowMore() {
    const nextPage = page + 1;
    setIsLoadingMore(true);
    setLoadMoreError(null);
    try {
      const nextMovies = await movieService.getMovies({
        page: nextPage,
        sortBy: CATALOG_SORT,
      });
      setMovies((prevMovies) => [
        ...prevMovies,
        ...nextMovies.filter(
          (movie) => !prevMovies.some((prev) => prev.id === movie.id)
        ),
      ]);
      setPage(nextPage);
      setHasMore(nextMovies.length > 0);
    } catch {
      setLoadMoreError("Не вдалося завантажити ще фільми. Спробуйте ще раз.");
    } finally {
      setIsLoadingMore(false);
    }
  }

  if (isLoading) {
    return <Spinner />;
  }

  if (error) {
    return <ErrorMessage message={error} />;
  }

  return (
    <div className={styles.page}>
      {topMovies.length > 0 && (
        <section>
          <h2 className={styles.sectionTitle}>Топ-20 фільмів</h2>
          <MovieCarousel movies={topMovies} />
        </section>
      )}

      <section>
        <h2 className={styles.sectionTitle}>Каталог</h2>
        {movies.length > 0 ? (
          <MovieGrid movies={movies} />
        ) : (
          <p className={styles.empty}>Фільмів поки немає</p>
        )}

        {loadMoreError && <ErrorMessage message={loadMoreError} />}

        {hasMore && movies.length > 0 && (
          <div className={styles.more}>
            <Button type="button" onClick={handleShowMore} disabled={isLoadingMore}>
              {isLoadingMore ? "Завантажуємо…" : "Показати ще"}
            </Button>
          </div>
        )}
      </section>
    </div>
  );
}

export default CatalogPage;