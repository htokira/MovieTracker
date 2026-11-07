import { useRef } from "react";
import type { ReactNode } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import styles from "../styles/HorizontalScroll.module.css";

interface HorizontalScrollProps {
  children: ReactNode;
}

function HorizontalScroll({ children }: HorizontalScrollProps) {
  const listRef = useRef<HTMLUListElement>(null);

  function scroll(direction: 1 | -1) {
    const list = listRef.current;
    if (!list) {
      return;
    }
    list.scrollBy({
      left: direction * list.clientWidth * 0.8,
      behavior: "smooth",
    });
  }

  return (
    <div className={styles.wrapper}>
      <button type="button" className={`${styles.arrow} ${styles.arrowLeft}`} onClick={() => scroll(-1)} aria-label="Прокрутити ліворуч">
        <ChevronLeft size={20} />
      </button>

      <ul ref={listRef} className={styles.list}>
        {children}
      </ul>

      <button type="button" className={`${styles.arrow} ${styles.arrowRight}`} onClick={() => scroll(1)} aria-label="Прокрутити праворуч">
        <ChevronRight size={20} />
      </button>
    </div>
  );
}

export default HorizontalScroll;