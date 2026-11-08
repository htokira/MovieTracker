import styles from "./SuccessMessage.module.css";

interface SuccessMessageProps {
  message: string;
}

function SuccessMessage({ message }: SuccessMessageProps) {
  return (
    <p className={styles.success} role="status">
      {message}
    </p>
  );
}

export default SuccessMessage;