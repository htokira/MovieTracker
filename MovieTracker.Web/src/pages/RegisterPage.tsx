import RegisterForm from "../features/auth/RegisterForm";
import styles from "../styles/AuthPage.module.css";

function RegisterPage() {
  return (
    <div className={styles.page}>
      <RegisterForm />
    </div>
  );
}

export default RegisterPage;