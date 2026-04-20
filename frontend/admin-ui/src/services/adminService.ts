import api from './api';

export interface DashboardStats {
  totalProducts: number;
  activeSuppliers: number;
  totalOrders: number;
}

export interface SystemStatus {
  backendConnected: boolean;
  gatewayConnected: boolean;
}

export const adminService = {
  getStats: async (): Promise<DashboardStats> => {
    const response = await api.get<DashboardStats>('/api/admin/dashboard-stats');
    return response.data;
  },

  getSystemStatus: async (): Promise<SystemStatus> => {
    const response = await api.get<SystemStatus>('/api/admin/system-status');
    return response.data;
  }
};