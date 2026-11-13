import { useEffect, useState } from "react";
import { Sparkles } from "lucide-react";
import Button from "@/components/Button/Button";
import { interpretTasteProfile } from "@/utils/tasteProfileInterpreter";
import { analyzeTasteProfile, getMyTasteProfile } from "@/services/tasteProfileService";
import type { TasteProfileDto } from "@/types/tasteProfile";
import styles from "./tasteProfile.module.css";

export function useTasteProfile() {
  const [profile, setProfile] = useState<TasteProfileDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let isCancelled = false;

    async function load() {
      try {
        const data = await getMyTasteProfile();
        if (!isCancelled) {
          setProfile(data);
        }
      } catch {
        if (!isCancelled) {
          setError("Не вдалося завантажити смаковий профіль");
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false);
        }
      }
    }

    load();

    return () => {
      isCancelled = true;
    };
  }, []);

  async function analyze() {
    setIsAnalyzing(true);
    setError("");

    try {
      const data = await analyzeTasteProfile();
      setProfile(data);
    } catch {
      setError("Не вдалося проаналізувати смаки. Спробуй ще раз");
    } finally {
      setIsAnalyzing(false);
    }
  }

  return { profile, isLoading, isAnalyzing, error, analyze };
}

function TasteProfile() {
  const { profile, isLoading, isAnalyzing, error, analyze } = useTasteProfile();

  const interpretation = profile ? interpretTasteProfile(profile) : null;

  let buttonText = "Проаналізувати мої смаки";
  if (isAnalyzing) {
    buttonText = "Аналізуємо...";
  } else if (profile) {
    buttonText = "Оновити аналіз";
  }

  return (
    <section className={styles.card} aria-labelledby="taste-title">
      <header className={styles.header}>
        <div className={styles.titleGroup}>
          <h2 id="taste-title" className={styles.title}>Мій смаковий профіль</h2>
          {profile && (
            <p className={styles.muted}>
              Востаннє оновлено:{" "}
              {new Date(profile.calculatedAt).toLocaleDateString("uk-UA")}
            </p>
          )}
        </div>

        {!isLoading && (
          <Button type="button" variant="primary" onClick={analyze} disabled={isAnalyzing}>
            <Sparkles size={16} />
            {buttonText}
          </Button>
        )}
      </header>

      {isLoading && <p className={styles.muted}>Завантаження...</p>}

      {!isLoading && !profile && (
        <p className={styles.muted}>Смаковий профіль ще не створено</p>
      )}

      {interpretation && profile && (
        <>
          <div className={styles.grid}>
            <article className={styles.item}>
              <p className={styles.label}>Звання</p>
              <h3 className={styles.value}>{interpretation.title}</h3>
              <p className={styles.text}>{interpretation.titleDescription}</p>
            </article>

            <article className={styles.item}>
              <p className={styles.label}>Архетип</p>
              <h3 className={styles.value}>{interpretation.archetype}</h3>
              <p className={styles.text}>{interpretation.archetypeDescription}</p>
            </article>

            <article className={styles.item}>
              <p className={styles.label}>Різноманіття смаку</p>
              <h3 className={styles.value}>{interpretation.diversityLabel}</h3>
              <p className={styles.text}>{interpretation.diversityDescription}</p>
            </article>
          </div>

          {profile.favoriteGenres.length > 0 && (
            <div className={styles.genres}>
              <h3 className={styles.label}>Улюблені жанри</h3>
              <ul className={styles.tags}>
                {profile.favoriteGenres.map((genre) => (
                  <li key={genre} className={styles.tag}>{genre}</li>
                ))}
              </ul>
            </div>
          )}
        </>
      )}

      {error && <p className={styles.error} role="alert">{error}</p>}
    </section>
  );
}

export default TasteProfile;