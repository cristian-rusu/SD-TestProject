export interface OrderDto {
  orderId: string;
  status: "Pending" | "Confirmed" | "Rejected";
  lines: OrderLineDto[];
}
export interface OrderLineDto {
  productId: string;
  quantity: number;
  acceptedPrice: number;
}
