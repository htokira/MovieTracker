import api from "./api";
import type { UserProfileResponse } from "../types/user";

export const userService = {
  getMyProfile: async (): Promise<UserProfileResponse> => {
    const response = await api.get<UserProfileResponse>("/users/me");
    return response.data;
  },
};