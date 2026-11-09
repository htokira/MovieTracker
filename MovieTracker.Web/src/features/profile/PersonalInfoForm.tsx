import { useState } from "react";
import type { ChangeEvent, FormEvent } from "react";
import Avatar from "@/components/Avatar/Avatar";
import Button from "@/components/Button/Button";
import ErrorMessage from "@/components/ErrorMessage/ErrorMessage";
import Input from "@/components/Input/Input";
import SuccessMessage from "@/components/SuccessMessage/SuccessMessage";
import Textarea from "@/components/TextArea/TextArea";
import { useAuth } from "@/features/auth/useAuth";
import { getErrorMessage } from "@/services/getErrorMessage";
import { profileService } from "@/services/profileService";
import type { UserProfileResponse } from "@/types/user";
import styles from "./ProfileForm.module.css";

interface PersonalInfoFormProps {
  user: UserProfileResponse;
}

interface PersonalInfoState {
  displayName: string;
  avatarUrl: string;
  bio: string;
}

function emptyToNull(value: string): string | null {
  const trimmed = value.trim();
  return trimmed === "" ? null : trimmed;
}

function PersonalInfoForm({ user }: PersonalInfoFormProps) {
  const { updateUser } = useAuth();

  const [input, setInput] = useState<PersonalInfoState>({
    displayName: user.displayName,
    avatarUrl: user.avatarUrl ?? "",
    bio: user.bio ?? "",
  });
  const [error, setError] = useState<string | null>(null);
  const [isSaved, setIsSaved] = useState<boolean>(false);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  function handleInputChange(
    e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) {
    setInput({ ...input, [e.target.name]: e.target.value });
    setIsSaved(false);
  }

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
    setIsSaved(false);
    setIsSubmitting(true);
    try {
      const updatedUser = await profileService.updateProfile({
        displayName: input.displayName.trim(),
        avatarUrl: emptyToNull(input.avatarUrl),
        bio: emptyToNull(input.bio),
      });
      updateUser(updatedUser);
      setIsSaved(true);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className={styles.form} onSubmit={handleSubmit}>
      <div className={styles.avatarRow}>
        <Avatar
          src={emptyToNull(input.avatarUrl)}
          name={input.displayName}
          size={72}
        />
        <div className={styles.avatarField}>
          <Input
            label="Посилання на аватарку"
            name="avatarUrl"
            type="url"
            placeholder="https://example.com/avatar.jpg"
            value={input.avatarUrl}
            onChange={handleInputChange}
            maxLength={500}
          />
        </div>
      </div>

      <Input
        label="Ім’я"
        name="displayName"
        type="text"
        placeholder="Як вас називати"
        value={input.displayName}
        onChange={handleInputChange}
        minLength={2}
        maxLength={50}
        required
      />

      <Input
        label="Email"
        name="email"
        type="email"
        hint="Email змінити не можна"
        value={user.email}
        disabled
        readOnly
      />

      <Textarea
        label="Про себе"
        name="bio"
        placeholder="Розкажіть кілька слів про себе"
        value={input.bio}
        onChange={handleInputChange}
        maxLength={500}
        rows={4}
      />

      {error && <ErrorMessage message={error} />}
      {isSaved && <SuccessMessage message="Дані успішно оновлено" />}

      <div className={styles.actions}>
        <Button type="submit" isLoading={isSubmitting}>
          Зберегти зміни
        </Button>
      </div>
    </form>
  );
}

export default PersonalInfoForm;