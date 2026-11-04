import { useState } from "react";
import type { ChangeEvent, SyntheticEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import Button from "../../components/Button";
import ErrorMessage from "../../components/ErrorMessage";
import Input from "../../components/Input";
import { getErrorMessage } from "../../services/getErrorMessage";
import type { RegisterRequest } from "../../types/auth";
import { useAuth } from "./useAuth";
import styles from "../../styles/AuthForm.module.css";

function RegisterForm() {
  const { register } = useAuth();
  const navigate = useNavigate();

  const [input, setInput] = useState<RegisterRequest>({
    displayName: "",
    email: "",
    password: "",
  });
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  function handleInputChange(e: ChangeEvent<HTMLInputElement>) {
    setInput({ ...input, [e.target.name]: e.target.value });
  }

  async function handleSubmit(e: SyntheticEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await register(input);
      navigate("/", { replace: true });
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className={styles.card}>
      <h1 className={styles.title}>Реєстрація</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <Input
          label="Ім’я"
          name="displayName"
          type="text"
          value={input.displayName}
          onChange={handleInputChange}
          autoComplete="nickname"
          minLength={2}
          maxLength={50}
          required
        />
        <Input
          label="Email"
          name="email"
          type="email"
          value={input.email}
          onChange={handleInputChange}
          autoComplete="email"
          maxLength={256}
          required
        />
        <Input
          label="Пароль"
          name="password"
          type="password"
          value={input.password}
          onChange={handleInputChange}
          autoComplete="new-password"
          minLength={8}
          maxLength={100}
          required
        />

        {error && <ErrorMessage message={error} />}

        <Button type="submit" fullWidth disabled={isSubmitting}>
          {isSubmitting ? "Створюємо акаунт…" : "Створити акаунт"}
        </Button>
      </form>

      <p className={styles.footer}>
        Вже є акаунт?{" "}
        <Link to="/login" className={styles.footerLink}>
          Увійти
        </Link>
      </p>
    </div>
  );
}

export default RegisterForm;