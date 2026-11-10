import type { TextareaHTMLAttributes } from "react";
import styles from "./textarea.module.css";
import fieldStyles from "@/components/Input/input.module.css";

interface TextAreaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label: string;
  name: string;
}

function TextArea({ label, name, ...rest }: TextAreaProps) {
  return (
    <div className={fieldStyles.field}>
      <label htmlFor={name} className={fieldStyles.label}>
        {label}
      </label>
      <textarea id={name} name={name} className={`${fieldStyles.input} ${styles.textarea}`} {...rest}/>
    </div>
  );
}

export default TextArea;