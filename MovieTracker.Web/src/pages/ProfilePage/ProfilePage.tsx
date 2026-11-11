import { LogOut } from "lucide-react";
import { useNavigate } from "react-router-dom";
import BackButton from "@/components/BackButton/BackButton";
import Button from "@/components/Button/Button";
import { useAuth } from "@/features/auth/useAuth";
import ChangePasswordForm from "@/features/profile/ChangePasswordForm";
import DeleteAccount from "@/features/profile/DeleteAccount";
import PersonalInfoForm from "@/features/profile/PersonalInfoForm";
import styles from "./profilePage.module.css";

function ProfilePage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  // спочатку на головну, щоб PrivateRoute не перекинув на /login
  function leaveProfile() {
    navigate("/", { replace: true });
    logout();
  }

  // сторінка лежить усередині PrivateRoute, тож user тут завжди є
  if (!user) {
    return null;
  }

  return (
    <section className={styles.page} aria-labelledby="profile-title">
      <header className={styles.header}>
        <div className={styles.titleGroup}>
          <BackButton />
          <h1 id="profile-title" className={styles.title}>Профіль</h1>
        </div>
        <Button type="button" variant="secondary" onClick={leaveProfile}>
          <LogOut size={16} />
          Вийти
        </Button>
      </header>

      <div className={styles.grid}>
        <section className={styles.card} aria-labelledby="personal-title">
          <h2 id="personal-title" className={styles.cardTitle}>Особисті дані</h2>
          <PersonalInfoForm user={user} />
        </section>

        <div className={styles.column}>
          <section className={styles.card} aria-labelledby="security-title">
            <h2 id="security-title" className={styles.cardTitle}>Безпека</h2>
            <ChangePasswordForm />
          </section>

          <section className={`${styles.card} ${styles.dangerCard}`} aria-labelledby="delete-title">
            <h2 id="delete-title" className={styles.cardTitle}>Видалення акаунту</h2>
            <DeleteAccount onDeleted={leaveProfile} />
          </section>
        </div>
      </div>
    </section>
  );
}

export default ProfilePage;