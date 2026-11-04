import { isAxiosError } from "axios";

export function getErrorMessage(error: unknown): string {
  if (isAxiosError(error)) {
    if (!error.response) {
      return "Не вдалося зв’язатися із сервером. Спробуйте пізніше.";
    }
    switch (error.response.status) {
      case 400:
        return "Перевірте введені дані та спробуйте ще раз.";
      case 401:
        return "Невірний email або пароль.";
      case 409:
        return "Акаунт з таким email уже існує.";
    }
  }
  return "Щось пішло не так. Спробуйте ще раз.";
}