import { ArrowLeft } from "lucide-react";
import { useNavigate } from "react-router-dom";
import Button from "@/components/Button/Button";
import styles from "./backButton.module.css";

interface BackButtonProps {
  to?: string; // без to повертає на попередню сторінку в історії
  label?: string;
}

function BackButton({ to, label = "Назад" }: BackButtonProps) {
  const navigate = useNavigate();

  function handleClick() {
    if (to) {
      navigate(to);
    } else {
      navigate(-1);
    }
  }

  return (
    <Button type="button" variant="secondary" className={styles.back} onClick={handleClick}>
      <ArrowLeft size={16} />
      {label}
    </Button>
  );
}

export default BackButton;