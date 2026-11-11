import RatingBadge from "@/components/RatingBadge/RatingBadge";
import type { ReviewResponse } from "@/types/review";
import styles from "./reviewList.module.css";

interface ReviewListProps {
  reviews: ReviewResponse[];
}

function formatDate(isoDate: string): string {
  return new Date(isoDate).toLocaleDateString("uk-UA", {
    day: "numeric",
    month: "long",
    year: "numeric",
  });
}

function ReviewList({ reviews }: ReviewListProps) {
  return (
    <ul className={styles.list}>
      {reviews.map((review) => (
        <li key={review.id}>
          <article className={styles.item}>
            <header className={styles.header}>
              <div>
                <p className={styles.author}>{review.authorName}</p>
                <p className={styles.date}>{formatDate(review.createdAt)}</p>
              </div>
              <p className={styles.rating}>
                <RatingBadge>
                  {review.rating}
                  <span className={styles.max}>/10</span>
                </RatingBadge>
              </p>
            </header>
            {review.text && <p className={styles.text}>{review.text}</p>}
          </article>
        </li>
      ))}
    </ul>
  );
}

export default ReviewList;