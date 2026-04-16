import { useState } from 'react';
import { HiRefresh, HiSearch } from 'react-icons/hi';
import { Card } from '../ui/Card';
import { Button } from '../ui/Button';
import { Input } from '../ui/Input';
import { Table, Pagination } from '../ui/Table';
import { StatusBadge } from '../ui/Badge';
import { useProducts } from '../../hooks/useProducts';
import type { Product } from '../../types';
import { formatCurrency, formatDate } from '../../utils/helpers';

export function ProductManagement() {
  const { result, loading, error, filter, setFilter, refetch, deleteProduct } = useProducts();
  const [search, setSearch] = useState('');

  const handleSearch = () => {
    setFilter((prev) => ({ ...prev, search, page: 1 }));
  };

  const columns = [
    { header: 'Name', accessorKey: 'name' as keyof Product },
    {
      header: 'Price',
      accessorKey: 'price' as keyof Product,
      cell: (row: Product) => formatCurrency(row.price),
    },
    {
      header: 'Qty',
      accessorKey: 'availableQuantity' as keyof Product,
      cell: (row: Product) => (
        <StatusBadge
          status={row.availableQuantity > 0 ? 'active' : 'inactive'}
        />
      ),
    },
    {
      header: 'Category',
      accessorKey: 'categoryName' as keyof Product,
      cell: (row: Product) => row.categoryName ?? '—',
    },
    {
      header: 'Supplier',
      accessorKey: 'supplierName' as keyof Product,
      cell: (row: Product) => row.supplierName ?? '—',
    },
    {
      header: 'Updated',
      accessorKey: 'updatedAt' as keyof Product,
      cell: (row: Product) => formatDate(row.updatedAt),
    },
    {
      header: 'Actions',
      cell: (row: Product) => (
        <Button
          variant="danger"
          size="sm"
          onClick={() => deleteProduct(row.id)}
        >
          Delete
        </Button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center gap-3">
        <div className="flex flex-1 items-center gap-2">
          <Input
            placeholder="Search products..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            className="max-w-xs"
          />
          <Button variant="outline" onClick={handleSearch}>
            <HiSearch className="h-4 w-4" />
          </Button>
        </div>
        <Button variant="outline" onClick={refetch}>
          <HiRefresh className="h-4 w-4" />
          Refresh
        </Button>
      </div>

      {error && (
        <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">
          {error}
        </div>
      )}

      <Card padding="none">
        <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4">
          <h3 className="font-semibold text-gray-900">
            Products{' '}
            {result && (
              <span className="text-sm font-normal text-gray-500">
                ({result.totalCount} total)
              </span>
            )}
          </h3>
        </div>
        <Table
          data={result?.items ?? []}
          columns={columns}
          loading={loading}
          emptyMessage="No products found. Import from a supplier to get started."
        />
        {result && result.totalPages > 1 && (
          <Pagination
            page={filter.page}
            totalPages={result.totalPages}
            onPageChange={(page) => setFilter((prev) => ({ ...prev, page }))}
          />
        )}
      </Card>
    </div>
  );
}
