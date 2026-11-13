import { Link, NavLink } from "react-router-dom";
import { Clapperboard } from "lucide-react";
import Avatar from "@/components/Avatar/Avatar";
import { useAuth } from "@/features/auth/useAuth";
import styles from "./Navbar.module.css";

function getLinkClassName({ isActive }: { isActive: boolean }) {
  return isActive ? `${styles.link} ${styles.active}` : styles.link;
}

function Navbar() {
  const { user, isLoading } = useAuth();

  function renderUserArea() {
    if (isLoading) {
      return null;
    }

    if (!user) {
      return (
        <Link to="/login" className={styles.loginLink}>
          Увійти
        </Link>
      );
    }

    return (
      <>
        <NavLink to="/my-movies" className={getLinkClassName}>
          Мої фільми
        </NavLink>
        <Link to="/profile" className={styles.avatarLink} aria-label="Мій профіль">
          <Avatar src={user.avatarUrl} name={user.displayName} />
        </Link>
      </>
    );
  }

  return (
    <header className={styles.navbar}>
      <nav className={styles.inner} aria-label="Головна навігація">
        <div className={styles.left}>
          <Link to="/" className={styles.logo}>
            <Clapperboard size={24} className={styles.logoIcon} />
            <span className={styles.logoText}>MovieTracker</span>
          </Link>
          <NavLink to="/" end className={getLinkClassName}>
            Каталог
          </NavLink>
        </div>

        <div className={styles.right}>{renderUserArea()}</div>
      </nav>
    </header>
  );
}

export default Navbar;