import styles from "./errorMessage.module.css";

interface ErrorMessageProps {
  message: string;
}

function ErrorMessage({ message }: ErrorMessageProps) {
  return (
    <p className={styles.error} role="alert">
      {message}
    </p>
  );
}

export default ErrorMessage;