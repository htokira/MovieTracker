import type { SelectHTMLAttributes } from "react";
import styles from "./Select.module.css";
import inputStyles from "@/components/Input/Input.module.css";

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  name: string;
}

function Select({ label, name, children, ...rest }: SelectProps) {
  return (
    <div className={inputStyles.field}>
      <label htmlFor={name} className={inputStyles.label}>
        {label}
      </label>
      <select id={name} name={name} className={`${inputStyles.input} ${styles.select}`} {...rest}>
        {children}
      </select>
    </div>
  );
}

export default Select;