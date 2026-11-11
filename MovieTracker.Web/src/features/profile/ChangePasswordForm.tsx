import { useState } from "react";
import type { ChangeEvent, FormEvent } from "react";
import { isAxiosError } from "axios";
import Button from "@/components/Button/Button";
import ErrorMessage from "@/components/ErrorMessage/ErrorMessage";
import Input from "@/components/Input/Input";
import SuccessMessage from "@/components/SuccessMessage/SuccessMessage";
import { PASSWORD_HINT, PASSWORD_PATTERN } from "@/features/auth/passwordRules";
import { getErrorMessage } from "@/services/getErrorMessage";
import { profileService } from "@/services/profileService";
import styles from "./profileForm.module.css";

interface PasswordState {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

const EMPTY_PASSWORDS: PasswordState = {
  currentPassword: "",
  newPassword: "",
  confirmPassword: "",
};

function getPasswordErrorMessage(error: unknown): string {
  if (isAxiosError(error) && error.response?.status === 400) {
    const data = error.response.data as
      | { errors?: Record<string, string[]> }
      | undefined;
    if (data?.errors && "PasswordMismatch" in data.errors) {
      return "Поточний пароль введено невірно.";
    }
    return "Новий пароль не відповідає вимогам безпеки.";
  }
  return getErrorMessage(error);
}

function ChangePasswordForm() {
  const [input, setInput] = useState<PasswordState>(EMPTY_PASSWORDS);
  const [error, setError] = useState<string | null>(null);
  const [isSaved, setIsSaved] = useState<boolean>(false);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  function handleInputChange(e: ChangeEvent<HTMLInputElement>) {
    setInput({ ...input, [e.target.name]: e.target.value });
    setIsSaved(false);
  }

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
    setIsSaved(false);

    if (input.newPassword !== input.confirmPassword) {
      setError("Паролі не збігаються.");
      return;
    }

    setIsSubmitting(true);
    try {
      await profileService.changePassword({
        currentPassword: input.currentPassword,
        newPassword: input.newPassword,
      });
      setInput(EMPTY_PASSWORDS);
      setIsSaved(true);
    } catch (err) {
      setError(getPasswordErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit}>
      <Input label="Поточний пароль" name="currentPassword" type="password" placeholder="Введіть поточний пароль" value={input.currentPassword}
        onChange={handleInputChange} autoComplete="current-password" required/>
      <Input label="Новий пароль" name="newPassword" type="password" placeholder="Придумайте новий пароль" hint={PASSWORD_HINT} value={input.newPassword}
        onChange={handleInputChange} autoComplete="new-password" pattern={PASSWORD_PATTERN} title={PASSWORD_HINT} minLength={8} maxLength={100} required/>
      <Input label="Підтвердження нового пароля" name="confirmPassword" type="password" placeholder="Повторіть новий пароль" value={input.confirmPassword}
        onChange={handleInputChange} autoComplete="new-password" required/>

      {error && <ErrorMessage message={error} />}
      {isSaved && <SuccessMessage message="Пароль успішно змінено" />}

      <div className={styles.actions}>
        <Button type="submit" isLoading={isSubmitting}>
          Змінити пароль
        </Button>
      </div>
    </form>
  );
}

export default ChangePasswordForm;