export interface FieldMapping {
  arrayRootPath: string;
  idPath: string;
  namePath: string;
  pricePath: string;
  catalogQuantityPath: string;
  descriptionPath: string;
  availabilityQuantityPath: string;
  checkoutConfirmationPath: string;
}

export interface Supplier {
  id: number;
  name: string;
  categoryId: number;
  supplierBaseUrl: string;
  catalogEndpoint: string;
  availabilityEndpoint: string;
  checkoutEndpoint: string;
  mappingConfig: FieldMapping | null;
  isActive: boolean;
  createdAt: string;
}

export interface CreateSupplierDto {
  name: string;
  categoryId: number;
  supplierBaseUrl: string;
  catalogEndpoint: string;
  availabilityEndpoint: string;
  checkoutEndpoint: string;
  mappingConfig: FieldMapping;
}

export interface UpdateSupplierDto extends Partial<CreateSupplierDto> {
  isActive?: boolean;
}