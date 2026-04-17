import api from './api';
import type { Supplier, CreateSupplierDto, UpdateSupplierDto, FieldMapping } from '../types';

export const supplierService = {
  getAll: async (): Promise<Supplier[]> => {
    const response = await api.get<Supplier[]>('/api/suppliers');
    return response.data;
  },

  getById: async (id: number): Promise<Supplier> => {
    const response = await api.get<Supplier>(`/api/suppliers/${id}`);
    return response.data;
  },

  create: async (data: CreateSupplierDto): Promise<Supplier> => {
    const response = await api.post<Supplier>('/api/suppliers', data);
    return response.data;
  },

  update: async (id: number, data: UpdateSupplierDto): Promise<Supplier> => {
    const response = await api.put<Supplier>(`/api/suppliers/${id}`, data);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/api/suppliers/${id}`);
  },

  saveMapping: async (id: number, mapping: FieldMapping): Promise<Supplier> => {
    const response = await api.post<Supplier>(`/api/suppliers/${id}/mapping`, mapping);
    return response.data;
  },

  fetchSampleJson: async (baseUrl: string, endpoint: string): Promise<Record<string, unknown>> => {
    const response = await api.post<Record<string, unknown>>('/api/suppliers/fetch-sample', {
      baseUrl,
      endpoint,
    });
    return response.data;
  },

  importProducts: async (id: number): Promise<{ imported: number }> => {
    const response = await api.post<{ imported: number }>(`/api/suppliers/${id}/import`);
    return response.data;
  },

  // NEW FIX: Added endpoint to sync availability
  syncAvailability: async (id: number): Promise<{ synced: number; updated: number }> => {
    const response = await api.post<{ synced: number; updated: number }>(`/api/suppliers/${id}/sync-availability`);
    return response.data;
  },
};