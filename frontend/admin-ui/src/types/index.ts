export type { Supplier, FieldMapping, CreateSupplierDto, UpdateSupplierDto } from './supplier';
export type { Product, ProductAttribute, Category, CreateProductDto, ProductFilter } from './product';
export type { Order, OrderItem, OrderStatus, UpdateOrderStatusDto, OrderFilter } from './order';

export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
