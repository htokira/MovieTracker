import { useState } from "react";
import type { ChangeEvent } from "react";
import { BookmarkPlus } from "lucide-react";
import Select from "@/components/Select/Select";
import styles from "./MovieTracking.module.css";

type WatchStatus = "none" | "planned" | "watched";

const STATUSES: { value: WatchStatus; label: string }[] = [
  { value: "none", label: "Не дивився" },
  { value: "planned", label: "В планах" },
  { value: "watched", label: "Переглянуто" },
];

// поки що статус просто в стані компоненета
function MovieTracking() {
  const [status, setStatus] = useState<WatchStatus>("none");

  function handleStatusChange(e: ChangeEvent<HTMLSelectElement>) {
    setStatus(e.target.value as WatchStatus);
  }

  return (
    <section className={styles.tracking} aria-label="Трекінг перегляду">
      <div className={styles.status}>
        <Select label="Мій статус" name="watchStatus" value={status} onChange={handleStatusChange}>
          {STATUSES.map((item) => (
            <option key={item.value} value={item.value}>
              {item.label}
            </option>
          ))}
        </Select>
      </div>

      <button type="button" className={styles.addButton} aria-label="Додати в колекцію" title="Додати в колекцію">
        <BookmarkPlus size={20} />
      </button>
    </section>
  );
}

export default MovieTracking;