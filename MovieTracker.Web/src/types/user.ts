export interface UserProfileResponse {
  id: string;
  email: string;
  displayName: string;
  avatarUrl: string | null;
  bio: string | null;
  createdAt: string;
}

export interface UpdateProfileRequest{
  displayName: string;
  avatarUrl: string | null;
  bio: string | null;
}

export interface ChangePasswordRequest{
  currentPassword: string;
  newPassword: string;
}