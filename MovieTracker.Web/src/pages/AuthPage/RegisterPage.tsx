import RegisterForm from "@/features/auth/AuthForm/RegisterForm";
import styles from "./AuthPage.module.css";

function RegisterPage() {
  return (
    <div className={styles.page}>
      <RegisterForm />
    </div>
  );
}

export default RegisterPage;