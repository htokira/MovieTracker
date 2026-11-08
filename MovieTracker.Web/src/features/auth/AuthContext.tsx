import { createContext, useEffect, useState } from "react";
import type { ReactNode } from "react";
import { authService } from "@/services/authService";
import { tokenStorage } from "@/services/tokenStorage";
import { userService } from "@/services/userService";
import type { LoginRequest, RegisterRequest } from "@/types/auth";
import type { UserProfileResponse } from "@/types/user";

interface AuthContextValue {
  user: UserProfileResponse | null;
  isLoading: boolean;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
  updateUser: (user: UserProfileResponse) => void;
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined
);

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<UserProfileResponse | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(
    () => tokenStorage.getAccessToken() !== null
  );

  useEffect(() => {
    if (!tokenStorage.getAccessToken()) {
      return;
    }

    async function loadProfile() {
      try {
        setUser(await userService.getMyProfile());
      } catch {
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    }

    loadProfile();
  }, []);

  async function login(data: LoginRequest) {
    const tokens = await authService.login(data);
    tokenStorage.setTokens(tokens.accessToken, tokens.refreshToken);
    setUser(await userService.getMyProfile());
  }

  async function register(data: RegisterRequest) {
    await authService.register(data);
    await login({ email: data.email, password: data.password });
  }

  function logout() {
    tokenStorage.clear();
    setUser(null);
  }

  function updateUser(updatedUser: UserProfileResponse) {
    setUser(updatedUser);
  }

  return (
    <AuthContext.Provider value={{ user, isLoading, login, register, logout, updateUser }}>
      {children}
    </AuthContext.Provider>
  );
}