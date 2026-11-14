import { useEffect, useState } from "react";
import type { ChangeEvent } from "react";
import ErrorMessage from "@/components/ErrorMessage/ErrorMessage";
import Select from "@/components/Select/Select";
import { movieTrackingService } from "@/services/movieTrackingService";
import { WATCH_STATUS_LABELS, WatchStatus } from "@/types/tracking";
import styles from "./movieTracking.module.css";

// Порожній рядок = "Не дивився" (статусу немає)
type StatusValue = WatchStatus | "";

interface WatchStatusSelectProps {
  movieId: number;
}

function WatchStatusSelect({ movieId }: WatchStatusSelectProps) {
  const [status, setStatus] = useState<StatusValue>("");
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isSaving, setIsSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let isIgnored = false;

    async function loadStatus() {
      setIsLoading(true);
      setError(null);
      setStatus("");
      try {
        const data = await movieTrackingService.getStatus(movieId);
        if (!isIgnored) {
          setStatus(data ? data.status : "");
        }
      } catch {
        if (!isIgnored) {
          setError("Не вдалося завантажити статус перегляду.");
        }
      } finally {
        if (!isIgnored) {
          setIsLoading(false);
        }
      }
    }

    loadStatus();

    return () => {
      isIgnored = true;
    };
  }, [movieId]);

  async function handleChange(event: ChangeEvent<HTMLSelectElement>) {
    const value = event.target.value as StatusValue;

    setIsSaving(true);
    setError(null);
    try {
      if (value === "") {
        await movieTrackingService.removeStatus(movieId);
      } else {
        await movieTrackingService.setStatus(movieId, { status: value });
      }
      // Статус міняємо лише після успішної відповіді сервера
      setStatus(value);
    } catch {
      // Статус не змінюється, користувач бачить повідомлення
      setError("Не вдалося змінити статус. Спробуйте пізніше.");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div className={styles.tracking}>
      <Select label="Мій статус" name="watchStatus" value={status} onChange={handleChange} disabled={isLoading || isSaving}>
        <option value="">Не дивився</option>
        <option value={WatchStatus.Watched}>{WATCH_STATUS_LABELS[WatchStatus.Watched]}</option>
        <option value={WatchStatus.Planned}>{WATCH_STATUS_LABELS[WatchStatus.Planned]}</option>
        <option value={WatchStatus.Dropped}>{WATCH_STATUS_LABELS[WatchStatus.Dropped]}</option>
      </Select>
      {error && <ErrorMessage message={error} />}
    </div>
  );
}

export default WatchStatusSelect;