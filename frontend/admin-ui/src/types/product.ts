export interface Product {
  id: number;
  providerId: number;
  categoryId: number;
  externalProductId: string;
  name: string;
  price: number;
  availableQuantity: number;
  categoryName?: string | null;
  supplierName?: string | null;
  description?: string | null;
  updatedAt?: string | null;
}
export interface ProductAttribute {
  id: number;
  productId: number;
  key: string;
  value: string;
}

export interface Category {
  id: number;
  name: string;
  description: string;
  productCount: number;
}

export interface CreateProductDto {
  name: string;
  description: string;
  price: number;
  availableQuantity: number;
  categoryId?: number;
}

export interface ProductFilter {
  search?: string;
  categoryId?: number;
  supplierId?: number;
  page: number;
  pageSize: number;
}
