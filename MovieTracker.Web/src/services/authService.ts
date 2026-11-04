import api from "./api";
import type { AuthResponse, LoginRequest, RegisterRequest } from "../types/auth";

export const authService = {
  register: async (data: RegisterRequest): Promise<void> => {
    await api.post("/auth/register", data);
  },

  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>("/auth/login", data);
    return response.data;
  },
};