import { useEffect, useState } from "react";
import type { ChangeEvent } from "react";
import { isAxiosError } from "axios";
import { useParams } from "react-router-dom";
import BackButton from "@/components/BackButton/BackButton";
import ErrorMessage from "@/components/ErrorMessage/ErrorMessage";
import Select from "@/components/Select/Select";
import Spinner from "@/components/Spinner/Spinner";
import { useAuth } from "@/features/auth/useAuth";
import CastList from "@/features/movie/CastList/CastList";
import MovieHero from "@/features/movie/MovieHero/MovieHero";
import ReviewSection from "@/features/review/ReviewSection/ReviewSection";
import { movieService } from "@/services/movieService";
import { movieTrackingService } from "@/services/movieTrackingService";
import type { MovieDetailDto } from "@/types/movie";
import { WATCH_STATUS_LABELS, WatchStatus } from "@/types/tracking";
import styles from "./movieDetailsPage.module.css";

// Порожній рядок = "Не дивився" (статусу немає)
type StatusValue = WatchStatus | "";

function MovieDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const movieId = Number(id);
  const isIdValid = Number.isInteger(movieId);
  const isAuthorized = Boolean(user);

  const [movie, setMovie] = useState<MovieDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const [status, setStatus] = useState<StatusValue>("");
  const [isStatusSaving, setIsStatusSaving] = useState<boolean>(false);
  const [statusError, setStatusError] = useState<string | null>(null);

  useEffect(() => {
    if (!isIdValid) {
      return;
    }

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
          setError(isNotFound ? "Фільм не знайдено." : "Не вдалося завантажити фільм. Спробуйте пізніше.");
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

  useEffect(() => {
    setStatus("");
    setStatusError(null);

    if (!isIdValid || !isAuthorized) {
      return;
    }

    let isIgnored = false;

    async function loadStatus() {
      try {
        const data = await movieTrackingService.getStatus(movieId);
        if (!isIgnored) {
          setStatus(data ? data.status : "");
        }
      } catch {
        if (!isIgnored) {
          setStatusError("Не вдалося завантажити статус перегляду.");
        }
      }
    }

    loadStatus();

    return () => {
      isIgnored = true;
    };
  }, [movieId, isIdValid, isAuthorized]);

  async function handleStatusChange(event: ChangeEvent<HTMLSelectElement>) {
    const value = event.target.value as StatusValue;

    setIsStatusSaving(true);
    setStatusError(null);
    try {
      if (value === "") {
        await movieTrackingService.removeStatus(movieId);
      } else {
        await movieTrackingService.setStatus(movieId, { status: value });
      }
      setStatus(value);
    } catch {
      setStatusError("Не вдалося змінити статус. Спробуйте пізніше.");
    } finally {
      setIsStatusSaving(false);
    }
  }

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
        {user && (
          <div className={styles.tracking}>
            <Select label="Мій статус" name="watchStatus" value={status} onChange={handleStatusChange} disabled={isStatusSaving}>
              <option value="">Не дивився</option>
              <option value={WatchStatus.Watched}>{WATCH_STATUS_LABELS[WatchStatus.Watched]}</option>
              <option value={WatchStatus.Planned}>{WATCH_STATUS_LABELS[WatchStatus.Planned]}</option>
              <option value={WatchStatus.Dropped}>{WATCH_STATUS_LABELS[WatchStatus.Dropped]}</option>
            </Select>
            {statusError && <ErrorMessage message={statusError} />}
          </div>
        )}
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
      <BackButton/>
      {renderContent()}
    </section>
  );
}

export default MovieDetailsPage;