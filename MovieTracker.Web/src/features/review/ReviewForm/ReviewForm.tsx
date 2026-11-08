import { useState } from "react";
import type { ChangeEvent, FormEvent } from "react";
import Button from "@/components/Button/Button";
import ErrorMessage from "@/components/ErrorMessage/ErrorMessage";
import Select from "@/components/Select/Select";
import TextArea from "@/components/TextArea/TextArea";
import { reviewService } from "@/services/reviewService";
import type { ReviewResponse } from "@/types/review";
import styles from "./ReviewForm.module.css";

const RATINGS = Array.from({ length: 10 }, (_, index) => index + 1);

interface ReviewFormProps {
  movieId: number;
  initialReview: ReviewResponse | null; // якщо користувач уже писав відгук
  onSaved: (review: ReviewResponse) => void;
}

function ReviewForm({ movieId, initialReview, onSaved }: ReviewFormProps) {
  const [rating, setRating] = useState<string>(
    initialReview ? String(initialReview.rating) : ""
  );
  const [text, setText] = useState<string>(initialReview?.text ?? "");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  function handleRatingChange(e: ChangeEvent<HTMLSelectElement>) {
    setRating(e.target.value);
  }

  function handleTextChange(e: ChangeEvent<HTMLTextAreaElement>) {
    setText(e.target.value);
  }

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      const trimmedText = text.trim();
      const savedReview = await reviewService.saveMyReview(movieId, {
        rating: Number(rating),
        text: trimmedText === "" ? null : trimmedText,
      });
      onSaved(savedReview);
    } catch {
      setError("Не вдалося зберегти відгук. Спробуйте ще раз.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit}>
      <h3 className={styles.title}>
        {initialReview ? "Ваш відгук" : "Залишити відгук"}
      </h3>

      <Select label="Оцінка" name="rating" value={rating} onChange={handleRatingChange} required>
        <option value="">Оберіть оцінку від 1 до 10</option>
        {RATINGS.map((value) => (
          <option key={value} value={value}>
            {value}
          </option>
        ))}
      </Select>

      <TextArea label="Відгук (необов’язково)" name="text" value={text} onChange={handleTextChange}
        placeholder="Поділіться враженнями про фільм" maxLength={2000} rows={4}/>

      {error && <ErrorMessage message={error} />}

      <div className={styles.actions}>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Зберігаємо…" : initialReview ? "Оновити відгук" : "Надіслати відгук"}
        </Button>
      </div>
    </form>
  );
}

export default ReviewForm;