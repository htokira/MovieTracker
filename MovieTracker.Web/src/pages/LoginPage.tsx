import LoginForm from "../features/auth/LoginForm";
import styles from "../styles/AuthPage.module.css";

function LoginPage() {
  return (
    <div className={styles.page}>
      <LoginForm />
    </div>
  );
}

export default LoginPage;