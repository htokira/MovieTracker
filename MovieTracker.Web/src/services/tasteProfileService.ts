import axios from "axios";
import api from "@/services/api";
import type { TasteProfileDto } from "@/types/tasteProfile";

// 404 - профіль ще не згенеровано
export async function getMyTasteProfile(): Promise<TasteProfileDto | null> {
  try {
    const { data } = await api.get<TasteProfileDto>("/taste-profile/me");
    return data;
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.status === 404) {
      return null;
    }
    throw error;
  }
}

export async function analyzeTasteProfile(): Promise<TasteProfileDto> {
  const { data } = await api.post<TasteProfileDto>("/taste-profile/analyze");
  return data;
}