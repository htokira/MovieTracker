import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import Avatar from "@/components/Avatar/Avatar";
import { feedService } from "@/services/feedService";
import type { FeedItemDto } from "@/services/feedService";
import styles from "./FeedPage.module.css";

const STATUS_LABELS: Record<string, string> = {
  planned: "додав(ла) у список «Хочу подивитись»",
  watching: "почав(ла) дивитись",
  watched: "подивився(лась)",
  dropped: "покинув(ла)",
};

function describeStatus(status: FeedItemDto["status"]): string {
  if (typeof status === "string") {
    const label = STATUS_LABELS[status.toLowerCase()];
    if (label) return label;
    return `змінив(ла) статус на «${status}»`;
  }
  return "змінив(ла) статус фільму";
}

function formatDate(value: string): string {
  return new Date(value).toLocaleString("uk-UA", {
    day: "numeric",
    month: "long",
    hour: "2-digit",
    minute: "2-digit",
  });
}

function FeedPage() {
  const [items, setItems] = useState<FeedItemDto[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadPage = useCallback(async (pageToLoad: number) => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await feedService.getFeed(pageToLoad);
      setItems((prev) => (pageToLoad === 1 ? data.items : [...prev, ...data.items]));
      setPage(data.page);
      setTotalPages(data.totalPages);
    } catch {
      setError("Не вдалося завантажити стрічку. Спробуйте пізніше.");
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadPage(1);
  }, [loadPage]);

  return (
    <section className={styles.page}>
      <h1 className={styles.title}>Стрічка</h1>

      {error && <p className={styles.error}>{error}</p>}

      {!isLoading && !error && items.length === 0 && (
        <p className={styles.empty}>
          Тут поки порожньо. Підпишіться на користувачів, і їхня активність з'явиться тут.
        </p>
      )}

      <ul className={styles.list}>
        {items.map((item, index) => (
          <li key={`${item.userId}-${item.movieId}-${item.type}-${index}`} className={styles.card}>
            <Link to={`/movie/${item.movieId}`} className={styles.posterLink}>
              {item.posterUrl ? (
                <img src={item.posterUrl} alt={item.movieTitle} className={styles.poster} />
              ) : (
                <div className={styles.posterPlaceholder} />
              )}
            </Link>

            <div className={styles.body}>
              <div className={styles.header}>
                <Avatar src={item.avatarUrl ?? null} name={item.displayName} />
                <div>
                  <p className={styles.text}>
                    <strong>{item.displayName}</strong>{" "}
                    {item.type === "review"
                      ? `поставив(ла) оцінку ${item.rating ?? ""} фільму`
                      : describeStatus(item.status)}{" "}
                    <Link to={`/movie/${item.movieId}`} className={styles.movieLink}>
                      {item.movieTitle}
                    </Link>
                  </p>
                  <span className={styles.date}>{formatDate(item.createdAt)}</span>
                </div>
              </div>

              {item.type === "review" && item.reviewText && (
                <p className={styles.review}>{item.reviewText}</p>
              )}
            </div>
          </li>
        ))}
      </ul>

      {isLoading && <p className={styles.empty}>Завантаження...</p>}

      {!isLoading && page < totalPages && (
        <button type="button" className={styles.moreButton} onClick={() => loadPage(page + 1)}>
          Завантажити ще
        </button>
      )}
    </section>
  );
}

export default FeedPage;