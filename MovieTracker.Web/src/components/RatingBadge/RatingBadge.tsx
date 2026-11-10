import type { ReactNode } from "react";
import { Star } from "lucide-react";
import styles from "./ratingBadge.module.css";

interface RatingBadgeProps {
  children: ReactNode;
  size?: number; // розмір зірки
}

function RatingBadge({ children, size = 16 }: RatingBadgeProps) {
  return (
    <span className={styles.rating}>
      <Star size={size} fill="currentColor" />
      {children}
    </span>
  );
}

export default RatingBadge;