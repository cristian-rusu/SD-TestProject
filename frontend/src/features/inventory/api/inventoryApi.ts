import { apiRequest } from "../../../shared/api/apiClient";
import type { StockDto } from "../types/inventory";
const path = (id: string) => `/inventory/products/${encodeURIComponent(id)}`;
export const getStock = (id: string) => apiRequest<StockDto>(path(id));
export const replenishStock = (id: string, quantity: number) =>
  apiRequest<void>(`${path(id)}/replenish`, {
    method: "POST",
    body: JSON.stringify({ quantity }),
  });
