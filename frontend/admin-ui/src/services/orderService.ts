import api from './api';

// Defined locally to guarantee the UI has the exact fields the new Controller DTO sends
export interface OrderItem {
  id: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface Order {
  id: number;
  customerName: string;
  totalAmount: number;
  status: string;
  confirmationCode: string;
  createdAt: string;
  items: OrderItem[];
}

export interface OrderFilter {
  page: number;
  pageSize: number;
  search?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  totalPages: number;
}

export const orderService = {
  getAll: async (filter: OrderFilter): Promise<PaginatedResponse<Order>> => {
    // Calling the exact route matching [Route("api/[controller]")]
    const response = await api.get<PaginatedResponse<Order>>('/api/order', {
      params: filter,
    });
    return response.data;
  },

  getById: async (id: number): Promise<Order> => {
    const response = await api.get<Order>(`/api/order/${id}`);
    return response.data;
  }
};