import { useEffect, useState } from "react";
import { Star } from "lucide-react";
import { Link } from "react-router-dom";
import Button from "@/components/Button/Button";
import ErrorMessage from "@/components/ErrorMessage/ErrorMessage";
import Spinner from "@/components/Spinner/Spinner";
import { reviewService } from "@/services/reviewService";
import type { MovieReviewsResponse, ReviewResponse } from "../../../types/review";
import { useAuth } from "@/features/auth/useAuth";
import ReviewForm from "@/features/review/ReviewForm/ReviewForm";
import ReviewList from "@/features/review/ReviewList/ReviewList";
import styles from "./ReviewSection.module.css";

const PREVIEW_COUNT = 3;

interface ReviewSectionProps {
  movieId: number;
}

function ReviewSection({ movieId }: ReviewSectionProps) {
  const { user, isLoading: isAuthLoading } = useAuth();

  const [reviewsData, setReviewsData] = useState<MovieReviewsResponse | null>(
    null
  );
  const [myReview, setMyReview] = useState<ReviewResponse | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isExpanded, setIsExpanded] = useState<boolean>(false);
  const [reloadKey, setReloadKey] = useState<number>(0);

  useEffect(() => {
    let isIgnored = false;

    async function loadReviews() {
      try {
        const data = await reviewService.getReviews(movieId);
        if (!isIgnored) {
          setReviewsData(data);
          setError(null);
        }
      } catch {
        if (!isIgnored) {
          setError("Не вдалося завантажити відгуки.");
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
  }, [movieId, reloadKey]);

  // власний відгук
  const userId = user?.id;
  useEffect(() => {
    if (!userId) {
      return;
    }

    let isIgnored = false;

    async function loadMyReview() {
      try {
        const review = await reviewService.getMyReview(movieId);
        if (!isIgnored) {
          setMyReview(review);
        }
      } catch {
        if (!isIgnored) {
          setMyReview(null);
        }
      }
    }

    loadMyReview();

    return () => {
      isIgnored = true;
    };
  }, [movieId, userId]);

  const ownReview = userId ? myReview : null;

  function handleSaved(review: ReviewResponse) {
    setMyReview(review);
    setReviewsData((prevData) =>
      prevData
        ? {
            ...prevData,
            items: [
              review,
              ...prevData.items.filter((item) => item.id !== review.id),
            ],
          }
        : prevData
    );
    setReloadKey((prevKey) => prevKey + 1);
  }

  function handleDeleted() {
    const deletedId = ownReview?.id;
    setMyReview(null);
    setReviewsData((prevData) =>
      prevData
        ? {
            ...prevData,
            items: prevData.items.filter((item) => item.id !== deletedId),
          }
        : prevData
    );
    setReloadKey((prevKey) => prevKey + 1);
  }

  function renderForm() {
    if (isAuthLoading) {
      return null;
    }
    if (!user) {
      return (
        <p className={styles.hint}>
          <Link to="/login" state={{ from: `/movie/${movieId}` }} className={styles.link}>
            Увійдіть
          </Link>
            , щоб залишити відгук
        </p>
      );
    }
    return (
      <ReviewForm key={ownReview?.id ?? "new"} movieId={movieId} initialReview={ownReview} onSaved={handleSaved} onDeleted={handleDeleted}/>
    );
  }

  function renderReviews() {
    if (isLoading) {
      return <Spinner />;
    }
    if (error) {
      return <ErrorMessage message={error} />;
    }
    if (!reviewsData || reviewsData.items.length === 0) {
      return (
        <p className={styles.empty}>Відгуків поки немає. Станьте першим!</p>
      );
    }
    return <ReviewList reviews={reviewsData.items} />;
  }

  return (
    <section className={styles.section}>
      <h2 className={styles.title}>Відгуки</h2>

      {reviewsData && reviewsData.totalReviews > 0 && (
        <p className={styles.summary}>
          <Star size={24} fill="currentColor" />
          {reviewsData.averageRating}
          <span className={styles.count}>
            · Відгуків: {reviewsData.totalReviews}
          </span>
        </p>
      )}

      {renderForm()}
      {renderReviews()}
    </section>
  );
}

export default ReviewSection;