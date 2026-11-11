// Додати нові елементи в кінець списку. Пропускаєм ті що вже є за id
export function appendUniqueById<T extends { id: number | string }>(
  items: T[],
  newItems: T[]
): T[] {
  return [
    ...items,
    ...newItems.filter(
      (newItem) => !items.some((item) => item.id === newItem.id)
    ),
  ];
}