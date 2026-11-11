import { useState } from "react";
import { Plus } from "lucide-react";
import Button from "@/components/Button/Button";
import CollectionCard from "@/features/collection/CollectionCard/CollectionCard";
import styles from "./collectionPage.module.css";

interface Collection {
  id: number;
  title: string;
  count: number;
}

// тимчасові дані бо колекції на беку все ще нема
const INITIAL_COLLECTIONS: Collection[] = [
  { id: 1, title: "Улюблені", count: 12 },
  { id: 2, title: "Переглянуті", count: 45 },
  { id: 3, title: "Буду дивитися", count: 5 },
];

function CollectionPage() {
  const [collections, setCollections] =
    useState<Collection[]>(INITIAL_COLLECTIONS);

  // заглушка
  function handleCreate() {
    window.alert("Створення колекцій буде доступне пізніше");
  }

  // поки що видаляє колекцію лише зі стану сторінки
  function handleDelete(id: number) {
    setCollections((prevCollections) =>
      prevCollections.filter((collection) => collection.id !== id)
    );
  }

  return (
    <section className={styles.page} aria-labelledby="collections-title">
      <section className={styles.header}>
        <h1 id="collections-title">Мої колекції</h1>
        <Button type="button" onClick={handleCreate}>
          <Plus size={18} />
          Створити нову колекцію
        </Button>
      </section>

      <section aria-label="Список колекцій">
        {collections.length > 0 ? (
          <ul className={styles.grid}>
            {collections.map((collection) => (
              <li key={collection.id}>
                <CollectionCard
                  title={collection.title}
                  moviesCount={collection.count}
                  onDelete={() => handleDelete(collection.id)}
                />
              </li>
            ))}
          </ul>
        ) : (
          <p className={styles.empty}>У вас ще немає колекцій</p>
        )}
      </section>
    </section>
  );
}

export default CollectionPage;