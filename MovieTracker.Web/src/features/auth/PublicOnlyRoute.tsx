import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "@/features/auth/useAuth";

interface LocationState {
  from?: string;
}

function PublicOnlyRoute() {
  const { user, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return null;
  }

  if (user) {
    const from = (location.state as LocationState | null)?.from ?? "/";
    return <Navigate to={from} replace />;
  }

  return <Outlet />;
}

export default PublicOnlyRoute;