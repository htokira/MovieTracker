import MovieCard from "@/features/movie/MovieCard/MovieCard";
import type { MovieListItemDto } from "@/types/movie";
import styles from "./movieGrid.module.css";

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