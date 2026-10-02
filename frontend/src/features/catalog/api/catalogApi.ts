import { apiRequest } from "../../../shared/api/apiClient";
import type { ProductDto, CreateProductRequest } from "../types/catalog";
const path = (id: string) => `/catalog/products/${encodeURIComponent(id)}`;
export const getProduct = (id: string) => apiRequest<ProductDto>(path(id));
export const createProduct = (body: CreateProductRequest) =>
  apiRequest<{ productId: string }>("/catalog/products/", {
    method: "POST",
    body: JSON.stringify(body),
  });
export const publishProduct = (id: string) =>
  apiRequest<void>(`${path(id)}/publish`, { method: "POST" });
export const discontinueProduct = (id: string) =>
  apiRequest<void>(`${path(id)}/discontinue`, { method: "POST" });
