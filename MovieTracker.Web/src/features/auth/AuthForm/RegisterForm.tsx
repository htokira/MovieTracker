import { useState } from "react";
import type { ChangeEvent, SyntheticEvent } from "react";
import { Link } from "react-router-dom";
import Button from "@/components/Button/Button";
import ErrorMessage from "@/components/ErrorMessage/ErrorMessage";
import Input from "@/components/Input/Input";
import { getErrorMessage } from "@/services/getErrorMessage";
import type { RegisterRequest } from "@/types/auth";
import { PASSWORD_HINT, PASSWORD_PATTERN } from "@/features/auth/passwordRules";
import { useAuth } from "@/features/auth/useAuth";
import styles from "./authForm.module.css";

interface RegisterFormState extends RegisterRequest {
  confirmPassword: string;
}

function RegisterForm() {
  const { register } = useAuth();
 
  const [input, setInput] = useState<RegisterFormState>({
    displayName: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  function handleInputChange(e: ChangeEvent<HTMLInputElement>) {
    setInput({ ...input, [e.target.name]: e.target.value });
  }

  async function handleSubmit(e: SyntheticEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);

    if (input.password !== input.confirmPassword) {
      setError("Паролі не збігаються.");
      return;
    }

    setIsSubmitting(true);
    try {
      await register({
        displayName: input.displayName,
        email: input.email,
        password: input.password,
      });
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className={styles.card}>
      <h1 className={styles.title}>Реєстрація</h1>
      <p className={styles.subtitle}>Створіть акаунт, щоб вести свою колекцію</p>

      <form className={styles.form} onSubmit={handleSubmit}>
        <Input label="Ім’я" name="displayName" type="text" placeholder="Введіть ім'я" value={input.displayName}
          onChange={handleInputChange} autoComplete="nickname" minLength={2} maxLength={50} required/>
        <Input label="Email" name="email" type="email" placeholder="example@gmail.com" value={input.email}
          onChange={handleInputChange} autoComplete="email" maxLength={256} required/>
        <Input label="Пароль" name="password" type="password" placeholder="Придумайте пароль" hint={PASSWORD_HINT} 
        value={input.password} onChange={handleInputChange} autoComplete="new-password" pattern={PASSWORD_PATTERN} title={PASSWORD_HINT} minLength={8} maxLength={100} required/>
        <Input label="Підтвердження пароля" name="confirmPassword" type="password" placeholder="Повторіть пароль" value={input.confirmPassword}
          onChange={handleInputChange} autoComplete="new-password" required/>

        {error && <ErrorMessage message={error} />}

        <Button type="submit" fullWidth isLoading={isSubmitting}>
          Створити акаунт
        </Button>
      </form>

      <p className={styles.footer}>
        Вже є акаунт?{" "}
        <Link to="/login" className={styles.footerLink}>
          Увійти
        </Link>
      </p>
    </section>
  );
}

export default RegisterForm;