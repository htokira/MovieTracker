import type { SelectHTMLAttributes } from "react";
import styles from "./select.module.css";
import fieldStyles from "@/components/Input/input.module.css";

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  name: string;
}

function Select({ label, name, children, ...rest }: SelectProps) {
  return (
    <div className={fieldStyles.field}>
      <label htmlFor={name} className={fieldStyles.label}>
        {label}
      </label>
      <select id={name} name={name} className={`${fieldStyles.input} ${styles.select}`} {...rest}>
        {children}
      </select>
    </div>
  );
}

export default Select;