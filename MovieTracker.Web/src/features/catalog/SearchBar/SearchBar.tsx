import type { ChangeEvent } from "react";
import { Search } from "lucide-react";
import styles from "./SearchBar.module.css";
import inputStyles from "@/components/Input/Input.module.css";

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
      <input type="search" className={`${inputStyles.input} ${styles.input}`} value={value} 
        onChange={handleChange} placeholder="Пошук фільмів за назвою" aria-label="Пошук фільмів"/>
    </div>
  );
}

export default SearchBar;