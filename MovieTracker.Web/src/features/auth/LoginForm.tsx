import { useState } from "react";
import type { ChangeEvent, SyntheticEvent } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import Button from "../../components/Button";
import ErrorMessage from "../../components/ErrorMessage";
import Input from "../../components/Input";
import { getErrorMessage } from "../../services/getErrorMessage";
import type { LoginRequest } from "../../types/auth";
import { useAuth } from "./useAuth";
import styles from "../../styles/AuthForm.module.css";

interface LocationState {
  from?: string;
}

function LoginForm() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as LocationState | null)?.from ?? "/";

  const [input, setInput] = useState<LoginRequest>({ email: "", password: "" });
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
      await login(input);
      navigate(from, { replace: true });
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className={styles.card}>
      <h1 className={styles.title}>Увійти в акаунт</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <Input
          label="Email"
          name="email"
          type="email"
          placeholder="example@gmail.com"
          value={input.email}
          onChange={handleInputChange}
          autoComplete="email"
          required
        />
        <Input
          label="Пароль"
          name="password"
          type="password"
          placeholder="Введіть пароль"
          value={input.password}
          onChange={handleInputChange}
          autoComplete="current-password"
          required
        />

        {error && <ErrorMessage message={error} />}

        <Button type="submit" fullWidth disabled={isSubmitting}>
          {isSubmitting ? "Входимо…" : "Увійти"}
        </Button>
      </form>

      <p className={styles.footer}>
        Ще немає акаунту?{" "}
        <Link to="/register" className={styles.footerLink}>
          Зареєструватися
        </Link>
      </p>
    </div>
  );
}

export default LoginForm;