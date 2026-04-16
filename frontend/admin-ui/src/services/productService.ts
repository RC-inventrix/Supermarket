import api from './api';
import type { Product, Category, CreateProductDto, ProductFilter, PaginatedResponse } from '../types';

export const productService = {
  getAll: async (filter: ProductFilter): Promise<PaginatedResponse<Product>> => {
    const response = await api.get<PaginatedResponse<Product>>('/api/products', {
      params: filter,
    });
    return response.data;
  },

  getById: async (id: number): Promise<Product> => {
    const response = await api.get<Product>(`/api/products/${id}`);
    return response.data;
  },

  create: async (data: CreateProductDto): Promise<Product> => {
    const response = await api.post<Product>('/api/products', data);
    return response.data;
  },

  update: async (id: number, data: Partial<CreateProductDto>): Promise<Product> => {
    const response = await api.put<Product>(`/api/products/${id}`, data);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/api/products/${id}`);
  },

  getCategories: async (): Promise<Category[]> => {
    const response = await api.get<Category[]>('/api/categories');
    return response.data;
  },

  createCategory: async (name: string, description?: string): Promise<Category> => {
    const response = await api.post<Category>('/api/categories', { name, description });
    return response.data;
  },
};
