import { useState } from "react";
import type { ChangeEvent, FormEvent } from "react";
import { Trash2 } from "lucide-react";
import Button from "@/components/Button/Button";
import ErrorMessage from "@/components/ErrorMessage/ErrorMessage";
import StarRating from "@/components/StarRating/StarRating";
import TextArea from "@/components/TextArea/TextArea";
import { reviewService } from "@/services/reviewService";
import type { ReviewResponse } from "@/types/review";
import { emptyToNull } from "@/utils/emptyToNull";
import styles from "./reviewForm.module.css";

interface ReviewFormProps {
  movieId: number;
  initialReview: ReviewResponse | null; // якщо користувач вже писав відгук
  onSaved: (review: ReviewResponse) => void;
  onDeleted: () => void;
}

function ReviewForm({ movieId, initialReview, onSaved, onDeleted}: ReviewFormProps) {
  const [rating, setRating] = useState<number>(initialReview?.rating ?? 0);
  const [text, setText] = useState<string>(initialReview?.text ?? "");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);

  function handleRatingChange(value: number) {
    setRating(value);
    setError(null);
  }
 
  function handleTextChange(e: ChangeEvent<HTMLTextAreaElement>) {
    setText(e.target.value);
  }
 
  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
 
    if (rating === 0) {
      setError("Оберіть оцінку від 1 до 10.");
      return;
    }
 
    setIsSubmitting(true);
    try {
      const savedReview = await reviewService.saveMyReview(movieId, {
        rating,
        text: emptyToNull(text),
      });
      onSaved(savedReview);
    } catch {
      setError("Не вдалося зберегти відгук. Спробуйте ще раз.");
    } finally {
      setIsSubmitting(false);
    }
  }
 
  async function handleDelete() {
    if (!window.confirm("Видалити ваш відгук?")) {
      return;
    }
    setError(null);
    setIsDeleting(true);
    try {
      await reviewService.deleteMyReview(movieId);
      onDeleted();
    } catch {
      setError("Не вдалося видалити відгук. Спробуйте ще раз.");
    } finally {
      setIsDeleting(false);
    }
  }
 
  return (
    <form className={styles.form} onSubmit={handleSubmit}>
      <h3>{initialReview ? "Ваш відгук" : "Залишити відгук"}</h3>
 
      <StarRating value={rating} onChange={handleRatingChange} />
 
      <TextArea label="Відгук (необов’язково)" name="text" value={text}
        onChange={handleTextChange} placeholder="Поділіться враженнями про фільм" maxLength={2000} rows={4} />
 
      {error && <ErrorMessage message={error} />}
 
      <div className={styles.actions}>
        <Button type="submit" isLoading={isSubmitting} disabled={isDeleting}>
          {initialReview ? "Оновити відгук" : "Надіслати відгук"}
        </Button>
 
        {initialReview && (
          <Button type="button" variant="danger" isLoading={isDeleting} disabled={isSubmitting} onClick={handleDelete}>
            <Trash2 size={16} />
            Видалити відгук
          </Button>
        )}
      </div>
    </form>
  );
}
 
export default ReviewForm;
 