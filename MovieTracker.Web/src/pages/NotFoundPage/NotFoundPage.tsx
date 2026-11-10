import { useNavigate } from "react-router-dom";
import BackButton from "@/components/BackButton/BackButton";
import Button from "@/components/Button/Button";
import styles from "./notFoundPage.module.css";

function NotFoundPage() {
  const navigate = useNavigate();

  return (
    <section className={styles.page}>
      <p className={styles.code}>404</p>
      <h1 className={styles.title}>Сторінку не знайдено</h1>
      <p className={styles.text}>
        Можливо, адресу введено з помилкою або сторінку більше не існує.
      </p>
      <div className={styles.actions}>
        <BackButton />
        <Button type="button" onClick={() => navigate("/")}>
          На головну
        </Button>
      </div>
    </section>
  );
}

export default NotFoundPage;