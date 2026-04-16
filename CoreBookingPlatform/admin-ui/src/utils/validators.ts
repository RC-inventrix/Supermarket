import { z } from 'zod';

export const supplierSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  supplierBaseUrl: z.string().url('Must be a valid URL'),
  productsEndpoint: z.string().min(1, 'Products endpoint is required'),
});

export const productSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  description: z.string().optional(),
  price: z.number().min(0, 'Price must be non-negative'),
  availableQuantity: z.number().int().min(0, 'Quantity must be non-negative'),
  categoryId: z.number().optional(),
});

export const fieldMappingSchema = z.object({
  nameField: z.string().min(1, 'Name field mapping is required'),
  priceField: z.string().min(1, 'Price field mapping is required'),
  availableQuantityField: z.string().min(1, 'Quantity field mapping is required'),
});

export type SupplierFormData = z.infer<typeof supplierSchema>;
export type ProductFormData = z.infer<typeof productSchema>;
export type FieldMappingFormData = z.infer<typeof fieldMappingSchema>;
