import { useState } from "react";
import { Trash2 } from "lucide-react";
import Button from "@/components/Button/Button";
import ErrorMessage from "@/components/ErrorMessage/ErrorMessage";
import { getErrorMessage } from "@/services/getErrorMessage";
import { profileService } from "@/services/profileService";
import styles from "./ProfileForm.module.css";

interface DeleteAccountProps {
  onDeleted: () => void;
}

function DeleteAccount({ onDeleted }: DeleteAccountProps) {
  const [error, setError] = useState<string | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);

  async function handleDelete() {
    if (!window.confirm("Видалити акаунт? Цю дію неможливо скасувати.")) {
      return;
    }
    setError(null);
    setIsDeleting(true);
    try {
      await profileService.deleteAccount();
      onDeleted();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <section className={styles.dangerZone}>
      <h3 className={styles.subtitle}>Видалення акаунту</h3>
      <p className={styles.text}>Цю дію неможливо скасувати.</p>

      {error && <ErrorMessage message={error} />}

      <Button
        type="button"
        variant="danger"
        isLoading={isDeleting}
        onClick={handleDelete}
      >
        <Trash2 size={16} />
        Видалити акаунт
      </Button>
    </section>
  );
}

export default DeleteAccount;