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
        <BackButton to="/" label="На головну" />
        {children}
      </div>
    </main>
  );
}

export default AuthPageLayout;