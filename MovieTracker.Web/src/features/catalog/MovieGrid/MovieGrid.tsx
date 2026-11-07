import MovieCard from "@/components/MovieCard/MovieCard";
import type { MovieListItemDto } from "@/types/movie";
import styles from "./MovieGrid.module.css";

interface MovieGridProps {
  movies: MovieListItemDto[];
}

function MovieGrid({ movies }: MovieGridProps) {
  return (
    <ul className={styles.grid}>
      {movies.map((movie) => (
        <li key={movie.id}>
          <MovieCard movie={movie} />
        </li>
      ))}
    </ul>
  );
}

export default MovieGrid;