export interface Supplier {
  id: number;
  name: string;
  supplierBaseUrl: string;
  productsEndpoint: string;
  mappingConfig: FieldMapping | null;
  isActive: boolean;
  createdAt: string;
}

export interface FieldMapping {
  nameField: string;
  priceField: string;
  availableQuantityField: string;
  additionalMappings?: Record<string, string>;
}

export interface CreateSupplierDto {
  name: string;
  supplierBaseUrl: string;
  productsEndpoint: string;
  mappingConfig?: FieldMapping;
}

export interface UpdateSupplierDto extends Partial<CreateSupplierDto> {
  isActive?: boolean;
}
