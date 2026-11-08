import LoginForm from "@/features/auth/AuthForm/LoginForm";
import styles from "./AuthPage.module.css";

function LoginPage() {
  return (
    <main className={styles.page}>
      <LoginForm />
    </main>
  );
}
 
export default LoginPage;