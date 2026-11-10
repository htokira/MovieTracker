import type { ButtonHTMLAttributes } from "react";
import { Loader2 } from "lucide-react";
import styles from "./button.module.css";

type ButtonVariant = "primary" | "secondary" | "danger";
 
interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  fullWidth?: boolean;
  isLoading?: boolean; // спінер і блок кнопки
}

function Button({ children, variant = "primary", fullWidth = false, isLoading = false, disabled, className, ...rest}: ButtonProps) {
  const classes = [styles.button, styles[variant], fullWidth && styles.fullWidth, className,]
    .filter(Boolean)
    .join(" ");
 
  return (
    <button className={classes} disabled={disabled || isLoading} {...rest}>
      {isLoading && <Loader2 size={16} className={styles.spinner} />}
      {children}
    </button>
  );
}
 
export default Button;