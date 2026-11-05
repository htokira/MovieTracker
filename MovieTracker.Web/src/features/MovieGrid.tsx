import MovieCard from "../components/MovieCard";
import type { MovieListItemDto } from "../types/movie";
import styles from "../styles/MovieGrid.module.css";

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