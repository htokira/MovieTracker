import { useEffect, useState } from "react";
import { isAxiosError } from "axios";
import { ArrowLeft } from "lucide-react";
import { Link, useParams } from "react-router-dom";
import ErrorMessage from "@/components/ErrorMessage/ErrorMessage";
import Spinner from "@/components/Spinner/Spinner";
import { useAuth } from "@/features/auth/useAuth";
import CastList from "@/features/CastList/CastList";
import MovieHero from "@/features/MovieHero/MovieHero";
import MovieTracking from "@/features/MovieTracking/MovieTracking";
import ReviewSection from "@/features/review/ReviewSection/ReviewSection";
import { movieService } from "@/services/movieService";
import type { MovieDetailDto } from "@/types/movie";
import styles from "./MovieDetailsPage.module.css";

function MovieDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const movieId = Number(id);
  const isIdValid = Number.isInteger(movieId);
 
  const [movie, setMovie] = useState<MovieDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
 
  useEffect(() => {
    if (!isIdValid) {
      return;
    }
 
    // Не оновлюємо стан, якщо компонент розмонтовано або id вже інший
    let isIgnored = false;
 
    async function loadMovie() {
      setIsLoading(true);
      setError(null);
      try {
        const data = await movieService.getMovieById(movieId);
        if (!isIgnored) {
          setMovie(data);
        }
      } catch (err) {
        if (!isIgnored) {
          const isNotFound = isAxiosError(err) && err.response?.status === 404;
          setError( isNotFound ? "Фільм не знайдено." : "Не вдалося завантажити фільм. Спробуйте пізніше.");
        }
      } finally {
        if (!isIgnored) {
          setIsLoading(false);
        }
      }
    }
 
    loadMovie();
 
    return () => {
      isIgnored = true;
    };
  }, [movieId, isIdValid]);
 
  function renderContent() {
    if (!isIdValid) {
      return <ErrorMessage message="Некоректний номер фільму." />;
    }
    if (isLoading) {
      return <Spinner />;
    }
    if (error) {
      return <ErrorMessage message={error} />;
    }
    if (!movie) {
      return null;
    }
    return (
      <>
        <MovieHero movie={movie}>
          {user && <MovieTracking />}
        </MovieHero>
        {movie.moviePersons.length > 0 && (
          <CastList people={movie.moviePersons} />
        )}
        <ReviewSection key={movie.id} movieId={movie.id} />
      </>
    );
  }
 
  return (
    <section className={styles.page}>
      <Link to="/" className={styles.back}>
        <ArrowLeft size={18} />
        Назад до каталогу
      </Link>
      {renderContent()}
    </section>
  );
}
 
export default MovieDetailsPage;