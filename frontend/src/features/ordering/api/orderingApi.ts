import { apiRequest } from "../../../shared/api/apiClient";
import type { OrderDto } from "../types/ordering";
export const getOrder = (id: string) =>
  apiRequest<OrderDto>(`/orders/${encodeURIComponent(id)}`);
export const placeOrder = (productId: string, quantity: number) =>
  apiRequest<{ orderId: string }>("/orders/", {
    method: "POST",
    body: JSON.stringify({ productId, quantity }),
  });
