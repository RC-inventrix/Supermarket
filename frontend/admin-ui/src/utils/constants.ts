export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:7000';

export const ORDER_STATUSES = [
  'Pending',
  'Confirmed',
  'Processing',
  'Shipped',
  'Delivered',
  'Cancelled',
] as const;

export const PAGE_SIZES = [10, 25, 50, 100] as const;

export const DEFAULT_PAGE_SIZE = 10;
