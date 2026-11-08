import { useEffect, useState, useRef } from "react";
import Button from "@/components/Button/Button";
import ErrorMessage from "@/components/ErrorMessage/ErrorMessage";
import Spinner from "@/components/Spinner/Spinner";
import MovieCarousel from "@/features/catalog/MovieCarousel/MovieCarousel";
import MovieGrid from "@/features/catalog/MovieGrid/MovieGrid";
import CatalogFilters from "@/features/catalog/CatalogFilters/CatalogFilters";
import SearchBar from "@/features/catalog/SearchBar/SearchBar";
import { useDebounce } from "@/hooks/useDebounce";
import { movieService } from "@/services/movieService";
import type { MovieListItemDto } from "@/types/movie";
import styles from "./CatalogPage.module.css";

const CATALOG_SORT = "vote_count.desc";
const SEARCH_DELAY_MS = 400;

function buildParams(query: string, genreId: number | null, page: number) {
  return {
    query: query || undefined,
    genreId: genreId ?? undefined,
    page,
    sortBy: CATALOG_SORT,
  };
}

function CatalogPage() {
  const [topMovies, setTopMovies] = useState<MovieListItemDto[]>([]);
  // фільтр + пошук із затримкою
  const [searchText, setSearchText] = useState<string>("");
  const [genreId, setGenreId] = useState<number | null>(null);
  const debouncedQuery = useDebounce(searchText.trim(), SEARCH_DELAY_MS);

  const [movies, setMovies] = useState<MovieListItemDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [hasMore, setHasMore] = useState<boolean>(true);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isLoadingMore, setIsLoadingMore] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [loadMoreError, setLoadMoreError] = useState<string | null>(null);

const requestVersion = useRef<number>(0);
 
  const isFiltering = debouncedQuery !== "" || genreId !== null;
  const hasActiveFilters = searchText !== "" || genreId !== null;
 
  useEffect(() => {
    let isIgnored = false;
 
    async function loadTopMovies() {
      try {
        const top = await movieService.getPopularMovies();
        if (!isIgnored) {
          setTopMovies(top);
        }
      } catch {
      }
    }
 
    loadTopMovies();
 
    return () => {
      isIgnored = true;
    };
  }, []);

  useEffect(() => {
    let isIgnored = false;
    requestVersion.current += 1;
 
    async function loadFirstPage() {
      setIsLoading(true);
      setError(null);
      setLoadMoreError(null);
      try {
        const firstPage = await movieService.getMovies(
          buildParams(debouncedQuery, genreId, 1)
        );
        if (!isIgnored) {
          setMovies(firstPage);
          setPage(1);
          setHasMore(firstPage.length > 0);
        }
      } catch {
        if (!isIgnored) {
          setMovies([]);
          setError("Не вдалося завантажити фільми. Спробуйте пізніше.");
        }
      } finally {
        if (!isIgnored) {
          setIsLoading(false);
        }
      }
    }
 
    loadFirstPage();
 
    return () => {
      isIgnored = true;
    };
  }, [debouncedQuery, genreId]);
 
  async function handleShowMore() {
    const version = requestVersion.current;
    const nextPage = page + 1;
    setIsLoadingMore(true);
    setLoadMoreError(null);
    try {
      const nextMovies = await movieService.getMovies(
        buildParams(debouncedQuery, genreId, nextPage)
      );
      if (version !== requestVersion.current) {
        return;
      }
      setMovies((prevMovies) => [
        ...prevMovies,
        ...nextMovies.filter(
          (movie) => !prevMovies.some((prev) => prev.id === movie.id)
        ),
      ]);
      setPage(nextPage);
      setHasMore(nextMovies.length > 0);
    } catch {
      if (version === requestVersion.current) {
        setLoadMoreError("Не вдалося завантажити ще фільми. Спробуйте ще раз.");
      }
    } finally {
      setIsLoadingMore(false);
    }
  }
 
  function handleReset() {
    setSearchText("");
    setGenreId(null);
  }
 
  function renderResults() {
    if (isLoading) {
      return <Spinner />;
    }
    if (error) {
      return <ErrorMessage message={error} />;
    }
    if (movies.length === 0) {
      return (
        <p className={styles.empty}>
          {isFiltering ? "Нічого не знайдено. Спробуйте змінити пошук або жанр." : "Фільмів поки немає"}
        </p>
      );
    }
    return (
      <>
        <MovieGrid movies={movies} />
 
        {loadMoreError && <ErrorMessage message={loadMoreError} />}
 
        {hasMore && (
          <div className={styles.more}>
            <Button type="button" onClick={handleShowMore} disabled={isLoadingMore}>
              {isLoadingMore ? "Завантажуємо…" : "Показати ще"}
            </Button>
          </div>
        )}
      </>
    );
  }
 
  return (
    <div className={styles.page}>
      {!isFiltering && topMovies.length > 0 && (
        <section>
          <h2 className={styles.sectionTitle}>Топ-20 фільмів</h2>
          <MovieCarousel movies={topMovies} />
        </section>
      )}
 
      <section>
        <h2 className={styles.sectionTitle}>Каталог</h2>
        <div className={styles.catalog}>
          <CatalogFilters genreId={genreId} hasActiveFilters={hasActiveFilters} onGenreChange={setGenreId} onReset={handleReset}/>
          <div className={styles.content}>
            <SearchBar value={searchText} onChange={setSearchText}/>
            {renderResults()}
          </div>
        </div>
      </section>
    </div>
  );
}
 
export default CatalogPage;