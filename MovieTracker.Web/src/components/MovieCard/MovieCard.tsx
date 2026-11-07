import { Link } from "react-router-dom";
import { Film, Star } from "lucide-react";
import type { MovieListItemDto } from "@/types/movie";
import styles from "./MovieCard.module.css";

interface MovieCardProps {
  movie: MovieListItemDto;
}

function MovieCard({ movie }: MovieCardProps) {
  const { id, title, posterUrl, releaseYear, rating } = movie;

  return (
    <Link to={`/movie/${id}`} className={styles.card}>
      {posterUrl ? (
        <img src={posterUrl} alt={title} className={styles.poster} loading="lazy"/>
      ) : (
        <div className={styles.posterPlaceholder}>
          <Film size={32} />
        </div>
      )}

      <div className={styles.info}>
        <h3 className={styles.title}>{title}</h3>
        <p className={styles.meta}>
          <span>{releaseYear || "—"}</span>
          <span className={styles.rating}>
            <Star size={14} fill="currentColor" />
            {rating > 0 ? rating.toFixed(1) : "—"}
          </span>
        </p>
      </div>
    </Link>
  );
}

export default MovieCard;