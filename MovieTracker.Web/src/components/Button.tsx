import type { ButtonHTMLAttributes } from "react";
import styles from "../styles/Button.module.css";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  fullWidth?: boolean;
}

function Button({ children, fullWidth = false, ...rest }: ButtonProps) {
  const className = fullWidth ? `${styles.button} ${styles.fullWidth}`: styles.button;

  return (
    <button className={className} {...rest}>
      {children}
    </button>
  );
}

export default Button;