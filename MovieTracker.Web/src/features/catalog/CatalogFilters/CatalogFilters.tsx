import type { ChangeEvent } from "react";
import { X } from "lucide-react";
import Select from "@/components/Select/Select";
import { GENRES } from "./genres";
import styles from "./CatalogFilters.module.css";

interface CatalogFiltersProps {
  genreId: number | null;
  hasActiveFilters: boolean;
  onGenreChange: (genreId: number | null) => void;
  onReset: () => void;
}

function CatalogFilters({
  genreId,
  hasActiveFilters,
  onGenreChange,
  onReset,
}: CatalogFiltersProps) {
  function handleGenreChange(e: ChangeEvent<HTMLSelectElement>) {
    const value = e.target.value;
    onGenreChange(value === "" ? null : Number(value));
  }

  return (
    <aside className={styles.filters}>
      <Select label="Жанр" name="genre" value={genreId ?? ""} onChange={handleGenreChange}>
        <option value="">Усі жанри</option>
        {GENRES.map((genre) => (
          <option key={genre.id} value={genre.id}>
            {genre.name}
          </option>
        ))}
      </Select>

      <button type="button" className={styles.reset} onClick={onReset} disabled={!hasActiveFilters}>
        <X size={16} />
        Скинути фільтри
      </button>
    </aside>
  );
}

export default CatalogFilters;