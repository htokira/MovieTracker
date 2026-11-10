import { Folder, Trash2 } from "lucide-react";
import styles from "./CollectionCard.module.css";

interface CollectionCardProps {
  title: string;
  moviesCount: number;
  onDelete: () => void;
}

function getMoviesLabel(count: number): string {
  const lastTwoDigits = count % 100;
  const lastDigit = count % 10;

  if (lastTwoDigits >= 11 && lastTwoDigits <= 14) {
    return "фільмів";
  }
  if (lastDigit === 1) {
    return "фільм";
  }
  if (lastDigit >= 2 && lastDigit <= 4) {
    return "фільми";
  }
  return "фільмів";
}

function CollectionCard({ title, moviesCount, onDelete }: CollectionCardProps) {
  return (
    <article className={styles.card}>
      <div className={styles.icon}>
        <Folder size={24} />
      </div>

      <div className={styles.info}>
        <h2 className={styles.title}>{title}</h2>
        <p className={styles.count}>
          {moviesCount} {getMoviesLabel(moviesCount)}
        </p>
      </div>

      <button type="button" className={styles.deleteButton} onClick={onDelete} aria-label={`Видалити колекцію ${title}`}>
        <Trash2 size={18} />
      </button>
    </article>
  );
}

export default CollectionCard;