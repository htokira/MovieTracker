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
      <BackButton />

      <header className={styles.header}>
        <h1 id="profile-title">Профіль</h1>
        <Button type="button" variant="secondary" onClick={leaveProfile}>
          <LogOut size={16} />
          Вийти
        </Button>
      </header>

      <section className={styles.card} aria-labelledby="personal-title">
        <h2 id="personal-title">Особисті дані</h2>
        <PersonalInfoForm user={user} />
      </section>

      <section className={styles.card} aria-labelledby="security-title">
        <h2 id="security-title">Безпека</h2>
        <ChangePasswordForm />
        <hr className={styles.divider} />
        <DeleteAccount onDeleted={leaveProfile} />
      </section>
    </section>
  );
}

export default ProfilePage;