import { User } from "lucide-react";
import HorizontalScroll from "@/components/HorizontalScroll/HorizontalScroll";
import Poster from "@/components/Poster/Poster";
import type { MoviePersonDto } from "@/types/movie";
import styles from "./castList.module.css";

interface CastListProps {
  people: MoviePersonDto[];
}

function CastList({ people }: CastListProps) {
  return (
    <section className={styles.section}>
      <h2>Акторський склад</h2>
      <HorizontalScroll>
        {people.map((person, index) => (
          <li key={`${person.fullName}-${index}`} className={styles.item}>
            <figure>
              <div className={styles.photo}>
                <Poster src={person.photoUrl} alt={person.fullName} fallbackIcon={<User size={32} />} />
              </div>
              <figcaption>
                <p className={styles.name}>{person.fullName}</p>
                <p className={styles.role}>{person.role}</p>
              </figcaption>
            </figure>
          </li>
        ))}
      </HorizontalScroll>
    </section>
  );
}

export default CastList;