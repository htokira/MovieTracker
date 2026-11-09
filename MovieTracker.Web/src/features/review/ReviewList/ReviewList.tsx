import { Star } from "lucide-react";
import type { ReviewResponse } from "@/types/review";
import styles from "./ReviewList.module.css";

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
                <Star size={16} fill="currentColor" />
                {review.rating}
                <span className={styles.max}>/10</span>
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