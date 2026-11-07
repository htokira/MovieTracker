import type { TextareaHTMLAttributes } from "react";
import styles from "./Textarea.module.css";
import inputStyles from "@/components/Input/Input.module.css";

interface TextAreaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label: string;
  name: string;
}

function TextArea({ label, name, ...rest }: TextAreaProps) {
  return (
    <div className={inputStyles.field}>
      <label htmlFor={name} className={inputStyles.label}>
        {label}
      </label>
      <textarea id={name} name={name} className={`${inputStyles.input} ${styles.textarea}`} {...rest}/>
    </div>
  );
}

export default TextArea;