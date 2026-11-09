import type { InputHTMLAttributes } from "react";
import styles from "./Input.module.css";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  name: string;
  hint?: string;
}

function Input({ label, name, hint, ...rest }: InputProps) {
  const hintId = `${name}-hint`;

  return (
    <div className={styles.field}>
      <label htmlFor={name} className={styles.label}>
        {label}
      </label>
      <input id={name} name={name} className={styles.input} aria-describedby={hint ? hintId : undefined} {...rest}/>
      {hint && (
        <p id={hintId} className={styles.hint}>
          {hint}
        </p>
      )}
    </div>
  );
}

export default Input;