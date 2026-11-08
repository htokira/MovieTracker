import api from "./api";
import type { ChangePasswordRequest, UpdateProfileRequest, UserProfileResponse } from "../types/user";

const PROFILE_URL = "/users/me";

export const profileService = {
  updateProfile: async (
    data: UpdateProfileRequest
  ): Promise<UserProfileResponse> => {
    const response = await api.put<UserProfileResponse>(PROFILE_URL, data);
    return response.data;
  },

  changePassword: async (data: ChangePasswordRequest): Promise<void> => {
    await api.post(`${PROFILE_URL}/change-password`, data);
  },

  deleteAccount: async (): Promise<void> => {
    await api.delete(PROFILE_URL);
  },
};