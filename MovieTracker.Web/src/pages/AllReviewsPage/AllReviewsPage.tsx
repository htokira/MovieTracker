import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import BackButton from "@/components/BackButton/BackButton";
import Button from "@/components/Button/Button";
import ErrorMessage from "@/components/ErrorMessage/ErrorMessage";
import Spinner from "@/components/Spinner/Spinner";
import ReviewList from "@/features/review/ReviewList/ReviewList";
import ReviewSummary from "@/features/review/ReviewSummary/ReviewSummary";
import { reviewService } from "@/services/reviewService";
import type { ReviewResponse } from "@/types/review";
import { appendUniqueById } from "@/utils/appendUniqueById";
import styles from "./allReviewsPage.module.css";

const PAGE_SIZE = 20;

function AllReviewsPage() {
  const { id } = useParams<{ id: string }>();
  const movieId = Number(id);
  const isIdValid = Number.isInteger(movieId) && movieId > 0;

  const [reviews, setReviews] = useState<ReviewResponse[]>([]);
  const [totalReviews, setTotalReviews] = useState<number>(0);
  const [averageRating, setAverageRating] = useState<number | null>(null);
  const [page, setPage] = useState<number>(1);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isLoadingMore, setIsLoadingMore] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [loadMoreError, setLoadMoreError] = useState<string | null>(null);

  useEffect(() => {
    if (!isIdValid) {
      return;
    }

    let isIgnored = false;

    async function loadReviews() {
      setIsLoading(true);
      setError(null);
      setLoadMoreError(null);
      try {
        const data = await reviewService.getReviews(movieId, 1, PAGE_SIZE);
        if (!isIgnored) {
          setReviews(data.items);
          setTotalReviews(data.totalReviews);
          setAverageRating(data.averageRating);
          setPage(1);
        }
      } catch {
        if (!isIgnored) {
          setError("Не вдалося завантажити відгуки. Спробуйте пізніше.");
        }
      } finally {
        if (!isIgnored) {
          setIsLoading(false);
        }
      }
    }

    loadReviews();

    return () => {
      isIgnored = true;
    };
  }, [movieId, isIdValid]);

  async function handleLoadMore() {
    const nextPage = page + 1;
    setIsLoadingMore(true);
    setLoadMoreError(null);
    try {
      const data = await reviewService.getReviews(movieId, nextPage, PAGE_SIZE);
      setReviews((prevReviews) => appendUniqueById(prevReviews, data.items));
      setTotalReviews(data.totalReviews);
      setAverageRating(data.averageRating);
      setPage(nextPage);
    } catch {
      setLoadMoreError("Не вдалося завантажити ще відгуки. Спробуйте ще раз.");
    } finally {
      setIsLoadingMore(false);
    }
  }

  function renderContent() {
    if (!isIdValid) {
      return <ErrorMessage message="Некоректний номер фільму." />;
    }
    if (isLoading) {
      return <Spinner />;
    }
    if (error) {
      return <ErrorMessage message={error} />;
    }
    if (reviews.length === 0) {
      return <p className={styles.empty}>Відгуків поки немає.</p>;
    }
    return (
      <>
        <ReviewSummary averageRating={averageRating} totalReviews={totalReviews}/>
        <ReviewList reviews={reviews} />

        {loadMoreError && <ErrorMessage message={loadMoreError} />}

        {reviews.length < totalReviews && (
          <div className={styles.more}>
            <Button type="button" variant="secondary" isLoading={isLoadingMore} onClick={handleLoadMore}>
              Завантажити ще
            </Button>
          </div>
        )}
      </>
    );
  }

  return (
    <section className={styles.page}>
      <BackButton to={isIdValid ? `/movie/${movieId}` : "/"} label={isIdValid ? "Назад до фільму" : "Назад до каталогу"}/>
      <h1>Усі відгуки</h1>
      {renderContent()}
    </section>
  );
}
 
export default AllReviewsPage;