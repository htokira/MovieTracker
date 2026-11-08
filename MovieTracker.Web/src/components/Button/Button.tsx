import type { ButtonHTMLAttributes } from "react";
import { Loader2 } from "lucide-react";
import styles from "./button.module.css";

type ButtonVariant = "primary" | "secondary" | "danger";
 
interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  fullWidth?: boolean;
  isLoading?: boolean;
}

function Button({ children, variant = "primary", fullWidth = false, isLoading = false, disabled, ...rest}: ButtonProps) {
  const className = [styles.button, styles[variant], fullWidth && styles.fullWidth]
    .filter(Boolean)
    .join(" ");
 
  return (
    <button className={className} disabled={disabled || isLoading} {...rest}>
      {isLoading && <Loader2 size={16} className={styles.spinner} />}
      {children}
    </button>
  );
}
 
export default Button;