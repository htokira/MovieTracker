import type { ReactNode } from "react";
import { Film } from "lucide-react";
import Poster from "@/components/Poster/Poster";
import RatingBadge from "@/components/RatingBadge/RatingBadge";
import type { MovieDetailDto } from "@/types/movie";
import styles from "./movieHero.module.css";

interface MovieHeroProps {
  movie: MovieDetailDto;
  children?: ReactNode; // додатковий блок під описом
}

function MovieHero({ movie, children }: MovieHeroProps) {
  const { title, posterUrl, releaseYear, rating, genres, description } = movie;

  return (
    <section className={styles.hero}>
      <div className={styles.posterWrapper}>
        <Poster src={posterUrl} alt={title} fallbackIcon={<Film size={48} />} />
      </div>

      <div className={styles.info}>
        <h1 className={styles.title}>{title}</h1>

        <p className={styles.meta}>
          <span>{releaseYear || "—"}</span>
          <RatingBadge>{rating > 0 ? rating.toFixed(1) : "—"}</RatingBadge>
        </p>

        {genres.length > 0 && (
          <ul className={styles.genres}>
            {genres.map((genre) => (
              <li key={genre.id} className={styles.genre}>
                {genre.name}
              </li>
            ))}
          </ul>
        )}

        {description ? (
          <p className={styles.description}>{description}</p>
        ) : (
          <p className={styles.noDescription}>Опис відсутній</p>
        )}

        {children && <div className={styles.actions}>{children}</div>}
      </div>
    </section>
  );
}

export default MovieHero;