export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  availableQuantity: number;
  categoryId: number | null;
  categoryName: string | null;
  supplierId: number | null;
  supplierName: string | null;
  attributes: ProductAttribute[];
  createdAt: string;
  updatedAt: string;
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
