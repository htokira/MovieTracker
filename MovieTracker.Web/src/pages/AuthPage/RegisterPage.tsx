import RegisterForm from "@/features/auth/AuthForm/RegisterForm";
import AuthPageLayout from "./AuthPageLayout";

function RegisterPage() {
  return (
    <AuthPageLayout>
      <RegisterForm />
    </AuthPageLayout>
  );
}

export default RegisterPage;