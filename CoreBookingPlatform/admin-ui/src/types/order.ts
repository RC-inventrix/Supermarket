export interface Order {
  id: number;
  customerId: string;
  customerName: string;
  customerEmail: string;
  status: OrderStatus;
  totalAmount: number;
  items: OrderItem[];
  createdAt: string;
  updatedAt: string;
}

export interface OrderItem {
  id: number;
  orderId: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export type OrderStatus = 'Pending' | 'Confirmed' | 'Processing' | 'Shipped' | 'Delivered' | 'Cancelled';

export interface UpdateOrderStatusDto {
  status: OrderStatus;
}

export interface OrderFilter {
  search?: string;
  status?: OrderStatus;
  page: number;
  pageSize: number;
}
