import { useState } from "react";
import { Star } from "lucide-react";
import styles from "./StarRating.module.css";

const STARS = Array.from({ length: 10 }, (_, index) => index + 1);

interface StarRatingProps {
  value: number;
  onChange: (value: number) => void;
}

function StarRating({ value, onChange }: StarRatingProps) {
  const [hoverValue, setHoverValue] = useState<number>(0);
  const displayValue = hoverValue || value;

  return (
    <fieldset className={styles.rating}>
      <legend className={styles.legend}>Оцінка</legend>

      <div className={styles.stars} role="radiogroup" aria-label="Оцінка від 1 до 10" onMouseLeave={() => setHoverValue(0)}>
        {STARS.map((star) => (
          <button key={star} type="button" role="radio" aria-checked={star === value} aria-label={`${star} з 10`}
            className={
              star <= displayValue
                ? `${styles.star} ${styles.active}`
                : styles.star
            }
            onClick={() => onChange(star)} onMouseEnter={() => setHoverValue(star)}>
            <Star size={24} fill={star <= displayValue ? "currentColor" : "none"} />
          </button>
        ))}
      </div>

      <p className={styles.caption}>
        {value > 0 ? `${value} з 10` : "Оберіть оцінку"}
      </p>
    </fieldset>
  );
}

export default StarRating;