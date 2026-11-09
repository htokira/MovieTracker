import RegisterForm from "@/features/auth/AuthForm/RegisterForm";
import styles from "./AuthPage.module.css";

function RegisterPage() {
  return (
    <main className={styles.page}>
      <RegisterForm />
    </main>
  );
}
 
export default RegisterPage;