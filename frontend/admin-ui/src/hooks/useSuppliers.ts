import { useState, useEffect, useCallback } from 'react';
import { supplierService } from '../services/supplierService';
import type { Supplier } from '../types';
import { toast } from 'react-toastify';

export function useSuppliers() {
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchSuppliers = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await supplierService.getAll();
      setSuppliers(data);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to fetch suppliers';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchSuppliers();
  }, [fetchSuppliers]);

  const deleteSupplier = async (id: number) => {
    try {
      await supplierService.delete(id);
      setSuppliers((prev) => prev.filter((s) => s.id !== id));
      toast.success('Supplier deleted successfully');
    } catch {
      toast.error('Failed to delete supplier');
    }
  };

  return { suppliers, loading, error, refetch: fetchSuppliers, deleteSupplier };
}
