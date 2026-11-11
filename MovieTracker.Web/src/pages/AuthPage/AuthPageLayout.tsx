import type { ReactNode } from "react";
import BackButton from "@/components/BackButton/BackButton";
import styles from "./authPage.module.css";

interface AuthPageLayoutProps {
  children: ReactNode;
}

function AuthPageLayout({ children }: AuthPageLayoutProps) {
  return (
    <main className={styles.page}>
      <div className={styles.content}>
        <nav className={styles.topBar} aria-label="Навігація">
          <BackButton to="/" label="На головну" />
        </nav>
        {children}
      </div>
    </main>
  );
}

export default AuthPageLayout;