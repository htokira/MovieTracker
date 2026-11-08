import { Link, NavLink } from "react-router-dom";
import { Bell, Clapperboard, User } from "lucide-react";
import styles from "./navbar.module.css";

function getLinkClassName({ isActive }: { isActive: boolean }) {
  return isActive ? `${styles.link} ${styles.active}` : styles.link;
}

function Navbar() {
  return (
    <header className={styles.navbar}>
      <nav className={styles.inner} aria-label="Головна навігація">
        <div className={styles.left}>
          <Link to="/" className={styles.logo}>
            <Clapperboard size={24} className={styles.logoIcon} />
            <span>Movie Tracker</span>
          </Link>
          <NavLink to="/" end className={getLinkClassName}>
            Каталог
          </NavLink>
        </div>

        <div className={styles.right}>
          <NavLink to="/collection" className={getLinkClassName}>
            Колекція
          </NavLink>
          <NavLink to="/login" className={getLinkClassName}>
            Увійти
          </NavLink>
          <button type="button" className={styles.iconButton} aria-label="Сповіщення">
            <Bell size={20} />
          </button>
          <Link to="/profile" className={styles.avatar} aria-label="Мій профіль">
            <User size={20} />
          </Link>
        </div>
      </nav>
    </header>
  );
}

export default Navbar;