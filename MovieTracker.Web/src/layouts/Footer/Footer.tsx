import styles from "./footer.module.css";

function Footer() {
  return (
    <footer className={styles.footer}>
      <div className={styles.inner}>
        <p>© 2026 Movie Tracker</p>

        <p className={styles.credits}>
          Цей застосунок використовує{" "}
          <a href="https://www.themoviedb.org" target="_blank" rel="noopener noreferrer" className={styles.link}>
            TMDB
          </a>{" "}
          API
        </p>
        <a href="https://github.com/htokira/MovieTracker" target="_blank" rel="noopener noreferrer" className={styles.link}>
          GitHub
        </a>
      </div>
    </footer>
  );
}

export default Footer;