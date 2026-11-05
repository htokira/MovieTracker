import { useRef } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import MovieCard from "../components/MovieCard";
import type { MovieListItemDto } from "../types/movie";
import styles from "../styles/MovieCarousel.module.css";

interface MovieCarouselProps {
  movies: MovieListItemDto[];
}

function MovieCarousel({ movies }: MovieCarouselProps) {
  const listRef = useRef<HTMLUListElement>(null);

  function scroll(direction: 1 | -1) {
    const list = listRef.current;
    if (!list) {
      return;
    }
    list.scrollBy({
      left: direction * list.clientWidth * 0.8,
      behavior: "smooth",
    });
  }

  return (
    <div className={styles.carousel}>
      <button type="button" className={`${styles.arrow} ${styles.arrowLeft}`} onClick={() => scroll(-1)} aria-label="Прокрутити ліворуч">
        <ChevronLeft size={20} />
      </button>

      <ul ref={listRef} className={styles.list}>
        {movies.map((movie) => (
          <li key={movie.id} className={styles.item}>
            <MovieCard movie={movie} />
          </li>
        ))}
      </ul>

      <button type="button" className={`${styles.arrow} ${styles.arrowRight}`} onClick={() => scroll(1)} aria-label="Прокрутити праворуч">
        <ChevronRight size={20} />
      </button>
    </div>
  );
}

export default MovieCarousel;