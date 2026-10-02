export interface ProductDto {
  productId: string;
  name: string;
  description: string;
  price: number;
  status: "Draft" | "Published" | "Discontinued";
}
export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
}
