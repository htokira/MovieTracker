import type { ChangeEvent } from "react";
import { Search } from "lucide-react";
import styles from "./searchBar.module.css";
import fieldStyles from "@/components/Input/input.module.css";

interface SearchBarProps {
  value: string;
  onChange: (value: string) => void;
}

function SearchBar({ value, onChange }: SearchBarProps) {
  function handleChange(e: ChangeEvent<HTMLInputElement>) {
    onChange(e.target.value);
  }

  return (
    <div className={styles.search}>
      <Search size={18} className={styles.icon} />
      <input type="search" className={`${fieldStyles.input} ${styles.input}`} value={value} 
        onChange={handleChange} placeholder="Пошук фільмів за назвою" aria-label="Пошук фільмів"/>
    </div>
  );
}

export default SearchBar;