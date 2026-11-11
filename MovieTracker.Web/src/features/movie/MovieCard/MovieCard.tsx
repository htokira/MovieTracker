import { Film } from "lucide-react";
import { Link } from "react-router-dom";
import Poster from "@/components/Poster/Poster";
import RatingBadge from "@/components/RatingBadge/RatingBadge";
import type { MovieListItemDto } from "@/types/movie";
import styles from "./movieCard.module.css";

interface MovieCardProps {
  movie: MovieListItemDto;
}

function MovieCard({ movie }: MovieCardProps) {
  const { id, title, posterUrl, releaseYear, rating } = movie;
 
  return (
    <Link to={`/movie/${id}`} className={styles.card}>
      <Poster src={posterUrl} alt={title} fallbackIcon={<Film size={32} />} />
 
      <div className={styles.info}>
        <h3 className={styles.title}>{title}</h3>
        <p className={styles.meta}>
          <span>{releaseYear || "—"}</span>
          <RatingBadge size={14}>
            {rating > 0 ? rating.toFixed(1) : "—"}
          </RatingBadge>
        </p>
      </div>
    </Link>
  );
}
 
export default MovieCard;