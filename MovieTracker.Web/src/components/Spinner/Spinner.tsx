import styles from "./Spinner.module.css";

function Spinner() {
  return (
    <div className={styles.wrapper}>
      <div className={styles.spinner} role="status" aria-label="Завантаження" />
    </div>
  );
}

export default Spinner;