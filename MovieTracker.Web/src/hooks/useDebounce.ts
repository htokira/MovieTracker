import { useEffect, useState } from "react";

// Повертає значення із затримкою. Оновлюється, коли value не змінювалось delay мс
export function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const timerId = setTimeout(() => setDebouncedValue(value), delay); // Якщо value змінилось раніше - скасувати таймер
    return () => clearTimeout(timerId);
  }, [value, delay]);

  return debouncedValue;
}