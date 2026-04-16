import api from './api';
import type { Order, UpdateOrderStatusDto, OrderFilter, PaginatedResponse } from '../types';

export const orderService = {
  getAll: async (filter: OrderFilter): Promise<PaginatedResponse<Order>> => {
    const response = await api.get<PaginatedResponse<Order>>('/api/orders', {
      params: filter,
    });
    return response.data;
  },

  getById: async (id: number): Promise<Order> => {
    const response = await api.get<Order>(`/api/orders/${id}`);
    return response.data;
  },

  updateStatus: async (id: number, data: UpdateOrderStatusDto): Promise<Order> => {
    const response = await api.put<Order>(`/api/orders/${id}/status`, data);
    return response.data;
  },
};
