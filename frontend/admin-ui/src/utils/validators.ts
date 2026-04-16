import { z } from 'zod';

export const fieldMappingSchema = z.object({
  arrayRootPath: z.string().optional(),
  idPath: z.string().min(1, 'Required'),
  namePath: z.string().min(1, 'Required'),
  pricePath: z.string().min(1, 'Required'),
  descriptionPath: z.string().optional(),
  catalogQuantityPath: z.string().min(1, 'Required'),
  availabilityQuantityPath: z.string().min(1, 'Required'),
  checkoutConfirmationPath: z.string().optional(),
});

export const supplierSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  categoryId: z.number().min(1, 'Category is required'),
  supplierBaseUrl: z.string().url('Must be a valid URL'),
  catalogEndpoint: z.string().min(1, 'Catalog endpoint is required'),
  availabilityEndpoint: z.string().min(1, 'Availability endpoint is required'),
  checkoutEndpoint: z.string().min(1, 'Checkout endpoint is required'),
});

export const productSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  description: z.string().optional(),
  price: z.number().min(0, 'Price must be non-negative'),
  availableQuantity: z.number().int().min(0, 'Quantity must be non-negative'),
  categoryId: z.number().optional(),
});

export type SupplierFormData = z.infer<typeof supplierSchema>;
export type ProductFormData = z.infer<typeof productSchema>;
export type FieldMappingFormData = z.infer<typeof fieldMappingSchema>;