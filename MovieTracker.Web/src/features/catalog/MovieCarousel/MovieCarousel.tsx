import HorizontalScroll from "@/components/HorizontalScroll/HorizontalScroll";
import MovieCard from "@/features/movie/MovieCard/MovieCard";
import type { MovieListItemDto } from "@/types/movie";
import styles from "./movieCarousel.module.css";

interface MovieCarouselProps {
  movies: MovieListItemDto[];
}

function MovieCarousel({ movies }: MovieCarouselProps) {
  return (
    <HorizontalScroll>
      {movies.map((movie) => (
        <li key={movie.id} className={styles.item}>
          <MovieCard movie={movie} />
        </li>
      ))}
    </HorizontalScroll>
  );
}

export default MovieCarousel;