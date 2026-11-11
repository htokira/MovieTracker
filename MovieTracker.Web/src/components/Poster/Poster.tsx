import type { ReactNode } from "react";
import styles from "./poster.module.css";

interface PosterProps {
  src: string | null;
  alt: string;
  fallbackIcon: ReactNode; // що показати, якщо зображення немає
}

function Poster({ src, alt, fallbackIcon }: PosterProps) {
  if (!src) {
    return <div className={styles.placeholder}>{fallbackIcon}</div>;
  }
  return <img src={src} alt={alt} className={styles.image} loading="lazy" />;
}

export default Poster;