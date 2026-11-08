import { BookmarkPlus, Film, Star } from "lucide-react";
import type { MovieDetailDto } from "@/types/movie";
import styles from "./MovieHero.module.css";
import Button from "@/components/Button/Button";

interface MovieHeroProps {
  movie: MovieDetailDto;
}

function MovieHero({ movie }: MovieHeroProps) {
  const { title, posterUrl, releaseYear, rating, genres, description } = movie;

  return (
    <section className={styles.hero}>
      <div className={styles.posterWrapper}>
        {posterUrl ? (
          <img src={posterUrl} alt={title} className={styles.poster} />
        ) : (
          <div className={styles.posterPlaceholder}>
            <Film size={48} />
          </div>
        )}
      </div>

      <div className={styles.info}>
        <h1 className={styles.title}>{title}</h1>

        <p className={styles.meta}>
          <span>{releaseYear || "—"}</span>
          <span className={styles.rating}>
            <Star size={16} fill="currentColor" />
            {rating > 0 ? rating.toFixed(1) : "—"}
          </span>
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

      <div className={styles.actions}>
          <Button type="button">
            <span className={styles.buttonContent}>
              <BookmarkPlus size={18} />
              Додати в колекцію
            </span>
          </Button>
        </div>
      </div>
    </section>
  );
}

export default MovieHero;