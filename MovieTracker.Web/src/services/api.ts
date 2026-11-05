import axios from "axios";
import type { AxiosError } from "axios";
import { tokenStorage } from "./tokenStorage";

const api = axios.create({
  baseURL: 'http://localhost:7226/api',
  headers: { "Content-Type": "application/json" },
});

api.interceptors.request.use((config) => {
  const token = tokenStorage.getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    const isUnauthorized = error.response?.status === 401;
    if (isUnauthorized && tokenStorage.getAccessToken()) {
      tokenStorage.clear();
      window.location.assign("/login");
    }
    return Promise.reject(error);
  }
);

export default api;