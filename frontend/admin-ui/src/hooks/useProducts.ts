import { useState, useEffect, useCallback } from 'react';
import { productService } from '../services/productService';
import type { Product, ProductFilter, PaginatedResponse } from '../types';
import { DEFAULT_PAGE_SIZE } from '../utils/constants';
import { toast } from 'react-toastify';

export function useProducts(initialFilter?: Partial<ProductFilter>) {
  const [result, setResult] = useState<PaginatedResponse<Product> | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<ProductFilter>({
    page: 1,
    pageSize: DEFAULT_PAGE_SIZE,
    ...initialFilter,
  });

  const fetchProducts = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await productService.getAll(filter);
      setResult(data);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to fetch products';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [filter]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const deleteProduct = async (id: number) => {
    try {
      await productService.delete(id);
      toast.success('Product deleted successfully');
      fetchProducts();
    } catch {
      toast.error('Failed to delete product');
    }
  };

  return { result, loading, error, filter, setFilter, refetch: fetchProducts, deleteProduct };
}
