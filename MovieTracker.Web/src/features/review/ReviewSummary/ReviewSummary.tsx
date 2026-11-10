import RatingBadge from "@/components/RatingBadge/RatingBadge";
import styles from "./reviewSummary.module.css";

interface ReviewSummaryProps {
  averageRating: number | null;
  totalReviews: number;
}

function ReviewSummary({ averageRating, totalReviews }: ReviewSummaryProps) {
  return (
    <p className={styles.summary}>
      <RatingBadge>{averageRating}</RatingBadge>
      <span className={styles.count}>· Відгуків: {totalReviews}</span>
    </p>
  );
}

export default ReviewSummary;