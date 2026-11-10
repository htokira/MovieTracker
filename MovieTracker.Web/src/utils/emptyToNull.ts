// бо отримуєм "" рядок, а для бекенду треба null
export function emptyToNull(value: string): string | null {
  const trimmed = value.trim();
  return trimmed === "" ? null : trimmed;
}