import { User } from "lucide-react";
import HorizontalScroll from "@/components/HorizontalScroll/HorizontalScroll";
import type { MoviePersonDto } from "@/types/movie";
import styles from "./CastList.module.css";

interface CastListProps {
  people: MoviePersonDto[];
}

function CastList({ people }: CastListProps) {
  return (
    <section>
      <h2 className={styles.title}>Акторський склад</h2>
      <HorizontalScroll>
        {people.map((person, index) => (
          <li key={`${person.fullName}-${index}`} className={styles.item}>
            {person.photoUrl ? (
              <img
                src={person.photoUrl}
                alt={person.fullName}
                className={styles.photo}
                loading="lazy"
              />
            ) : (
              <div className={styles.photoPlaceholder}>
                <User size={32} />
              </div>
            )}
            <p className={styles.name}>{person.fullName}</p>
            <p className={styles.role}>{person.role}</p>
          </li>
        ))}
      </HorizontalScroll>
    </section>
  );
}

export default CastList;