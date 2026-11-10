import LoginForm from "@/features/auth/AuthForm/LoginForm";
import AuthPageLayout from "./AuthPageLayout";

function LoginPage() {
  return (
    <AuthPageLayout>
      <LoginForm />
    </AuthPageLayout>
  );
}

export default LoginPage;