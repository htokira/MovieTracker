import { useEffect, useState } from "react";
import ErrorMessage from "@/components/ErrorMessage/ErrorMessage";
import Spinner from "@/components/Spinner/Spinner";
import PageHeader from "@/components/PageHeader/PageHeader";
import PaginatedMovieGrid from "@/components/PaginatedMovieGrid/PaginatedMovieGrid";
import { MY_MOVIES_PAGE_SIZE, movieTrackingService } from "@/services/movieTrackingService";
import type { MovieListItemDto } from "@/types/movie";
import { WATCH_STATUS_LABELS, WatchStatus } from "@/types/tracking";
import { appendUniqueById } from "@/utils/appendUniqueById";
import styles from "./myMoviesPage.module.css";

type TabValue = WatchStatus | "all";

interface Tab {
  value: TabValue;
  label: string;
}

const TABS: Tab[] = [
  { value: "all", label: "Усі" },
  { value: WatchStatus.Watched, label: WATCH_STATUS_LABELS[WatchStatus.Watched] },
  { value: WatchStatus.Planned, label: WATCH_STATUS_LABELS[WatchStatus.Planned] },
  { value: WatchStatus.Dropped, label: WATCH_STATUS_LABELS[WatchStatus.Dropped] },
];

function MyMoviesPage() {
  const [activeTab, setActiveTab] = useState<TabValue>("all");
  const [movies, setMovies] = useState<MovieListItemDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [total, setTotal] = useState<number>(0);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isLoadingMore, setIsLoadingMore] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [loadMoreError, setLoadMoreError] = useState<string | null>(null);

  const status = activeTab === "all" ? undefined : activeTab;
  const hasMore = page * MY_MOVIES_PAGE_SIZE < total;

  useEffect(() => {
    let isIgnored = false;

    async function loadFirstPage() {
      setIsLoading(true);
      setError(null);
      setLoadMoreError(null);
      try {
        const data = await movieTrackingService.getMyMovies(status, 1);
        if (!isIgnored) {
          setMovies(data.movies);
          setTotal(data.total);
          setPage(1);
        }
      } catch {
        if (!isIgnored) {
          setMovies([]);
          setError("Не вдалося завантажити ваші фільми. Спробуйте пізніше.");
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
  }, [status]);

  async function handleLoadMore() {
    const nextPage = page + 1;

    setIsLoadingMore(true);
    setLoadMoreError(null);
    try {
      const data = await movieTrackingService.getMyMovies(status, nextPage);
      setMovies((prev) => appendUniqueById(prev, data.movies));
      setTotal(data.total);
      setPage(nextPage);
    } catch {
      setLoadMoreError("Не вдалося завантажити ще фільми. Спробуйте пізніше.");
    } finally {
      setIsLoadingMore(false);
    }
  }

  function renderContent() {
    if (isLoading) {
      return <Spinner />;
    }
    if (error) {
      return <ErrorMessage message={error} />;
    }
    if (movies.length === 0) {
      return <p className={styles.empty}>Ви ще не додали жодного фільму</p>;
    }
    return (
      <PaginatedMovieGrid movies={movies} hasMore={hasMore} isLoadingMore={isLoadingMore}
        onLoadMore={handleLoadMore} loadMoreError={loadMoreError} emptyMessage="Ви ще не додали жодного фільму"/>
    );
  }

  return (
    <section className={styles.page}>
      <PageHeader showBackButton title="Мої фільми" />
      <div className={styles.tabs} role="group" aria-label="Фільтр за статусом перегляду">
        {TABS.map((tab) => (
          <button key={tab.value} type="button" className={tab.value === activeTab ? `${styles.tab} ${styles.tabActive}` : styles.tab}
           aria-pressed={tab.value === activeTab} onClick={() => setActiveTab(tab.value)} disabled={isLoadingMore}> 
           {tab.label}
          </button>
        ))}
      </div>
      {renderContent()}
    </section>
  );
}

export default MyMoviesPage;