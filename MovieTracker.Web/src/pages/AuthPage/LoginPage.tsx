import LoginForm from "@/features/auth/AuthForm/LoginForm";
import styles from "./AuthPage.module.css";

function LoginPage() {
  return (
    <div className={styles.page}>
      <LoginForm />
    </div>
  );
}

export default LoginPage;