import BackButton from "@/components/BackButton/BackButton";
import styles from "./pageHeader.module.css";

interface PageHeaderProps {
  title: string;
  showBackButton?: boolean;
}

function PageHeader({ title, showBackButton = false }: PageHeaderProps) {
  return (
    <header className={styles.header}>
      {showBackButton && <BackButton />}
      <h1 className={styles.title}>{title}</h1>
    </header>
  );
}

export default PageHeader;