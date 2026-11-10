import { Star } from "lucide-react";
import styles from "./ReviewSummary.module.css";

interface ReviewSummaryProps {
  averageRating: number | null;
  totalReviews: number;
}

function ReviewSummary({ averageRating, totalReviews }: ReviewSummaryProps) {
  return (
    <p className={styles.summary}>
      <Star size={16} fill="currentColor" />
      {averageRating}
      <span className={styles.count}>· Відгуків: {totalReviews}</span>
    </p>
  );
}

export default ReviewSummary;