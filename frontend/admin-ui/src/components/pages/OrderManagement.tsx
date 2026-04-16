import { useState, useEffect } from 'react';
import { HiSearch, HiRefresh } from 'react-icons/hi';
import { Card } from '../ui/Card';
import { Button } from '../ui/Button';
import { Input } from '../ui/Input';
import { Table, Pagination } from '../ui/Table';
import { StatusBadge } from '../ui/Badge';
import { Modal } from '../ui/Modal';
import { orderService } from '../../services/orderService';
import { useApiCall } from '../../hooks/useApiCall';
import type { Order, OrderStatus, OrderFilter, PaginatedResponse } from '../../types';
import { ORDER_STATUSES, DEFAULT_PAGE_SIZE } from '../../utils/constants';
import { formatCurrency, formatDate } from '../../utils/helpers';
import { toast } from 'react-toastify';

export function OrderManagement() {
  const [orders, setOrders] = useState<PaginatedResponse<Order> | null>(null);
  const [loading, setLoading] = useState(false);
  const [filter, setFilter] = useState<OrderFilter>({ page: 1, pageSize: DEFAULT_PAGE_SIZE });
  const [search, setSearch] = useState('');
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [detailsOpen, setDetailsOpen] = useState(false);
  const { execute: updateStatus, loading: updating } = useApiCall<Order>();

  const fetchOrders = async () => {
    setLoading(true);
    try {
      const data = await orderService.getAll(filter);
      setOrders(data);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Failed to fetch orders';
      toast.error(msg);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrders();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  const handleSearch = () => {
    setFilter((prev) => ({ ...prev, search, page: 1 }));
  };

  const handleStatusChange = async (order: Order, status: OrderStatus) => {
    await updateStatus(
      () => orderService.updateStatus(order.id, { status }),
      `Order #${order.id} status updated to ${status}`
    );
    fetchOrders();
  };

  const openDetails = (order: Order) => {
    setSelectedOrder(order);
    setDetailsOpen(true);
  };

  const columns = [
    {
      header: 'Order #',
      accessorKey: 'id' as keyof Order,
      cell: (row: Order) => (
        <button
          className="font-medium text-blue-600 hover:underline"
          onClick={() => openDetails(row)}
        >
          #{row.id}
        </button>
      ),
    },
    { header: 'Customer', accessorKey: 'customerName' as keyof Order },
    {
      header: 'Status',
      cell: (row: Order) => <StatusBadge status={row.status} />,
    },
    {
      header: 'Total',
      cell: (row: Order) => formatCurrency(row.totalAmount),
    },
    {
      header: 'Date',
      cell: (row: Order) => formatDate(row.createdAt),
    },
    {
      header: 'Update Status',
      cell: (row: Order) => (
        <select
          className="rounded-md border border-gray-300 px-2 py-1 text-xs focus:outline-none focus:ring-2 focus:ring-blue-500"
          value={row.status}
          onChange={(e) => handleStatusChange(row, e.target.value as OrderStatus)}
          disabled={updating}
        >
          {ORDER_STATUSES.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center gap-3">
        <div className="flex flex-1 items-center gap-2">
          <Input
            placeholder="Search by customer name..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
            className="max-w-xs"
          />
          <Button variant="outline" onClick={handleSearch}>
            <HiSearch className="h-4 w-4" />
          </Button>
        </div>
        <select
          className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          value={filter.status ?? ''}
          onChange={(e) =>
            setFilter((prev) => ({
              ...prev,
              status: (e.target.value as OrderStatus) || undefined,
              page: 1,
            }))
          }
        >
          <option value="">All Statuses</option>
          {ORDER_STATUSES.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>
        <Button variant="outline" onClick={fetchOrders}>
          <HiRefresh className="h-4 w-4" />
          Refresh
        </Button>
      </div>

      <Card padding="none">
        <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4">
          <h3 className="font-semibold text-gray-900">
            Orders{' '}
            {orders && (
              <span className="text-sm font-normal text-gray-500">
                ({orders.totalCount} total)
              </span>
            )}
          </h3>
        </div>
        <Table
          data={orders?.items ?? []}
          columns={columns}
          loading={loading}
          emptyMessage="No orders found."
        />
        {orders && orders.totalPages > 1 && (
          <Pagination
            page={filter.page}
            totalPages={orders.totalPages}
            onPageChange={(page) => setFilter((prev) => ({ ...prev, page }))}
          />
        )}
      </Card>

      {/* Order Details Modal */}
      <Modal
        isOpen={detailsOpen}
        onClose={() => setDetailsOpen(false)}
        title={`Order #${selectedOrder?.id} Details`}
        size="lg"
      >
        {selectedOrder && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-gray-500">Customer:</span>
                <span className="ml-2 font-medium">{selectedOrder.customerName}</span>
              </div>
              <div>
                <span className="text-gray-500">Email:</span>
                <span className="ml-2">{selectedOrder.customerEmail}</span>
              </div>
              <div>
                <span className="text-gray-500">Status:</span>
                <span className="ml-2">
                  <StatusBadge status={selectedOrder.status} />
                </span>
              </div>
              <div>
                <span className="text-gray-500">Date:</span>
                <span className="ml-2">{formatDate(selectedOrder.createdAt)}</span>
              </div>
            </div>

            <div>
              <h4 className="mb-2 font-medium text-gray-900">Line Items</h4>
              <table className="min-w-full divide-y divide-gray-200 rounded-lg border border-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-4 py-2 text-left text-xs font-medium text-gray-500">Product</th>
                    <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">Qty</th>
                    <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">Unit Price</th>
                    <th className="px-4 py-2 text-right text-xs font-medium text-gray-500">Total</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {selectedOrder.items.map((item) => (
                    <tr key={item.id}>
                      <td className="px-4 py-2 text-sm">{item.productName}</td>
                      <td className="px-4 py-2 text-right text-sm">{item.quantity}</td>
                      <td className="px-4 py-2 text-right text-sm">{formatCurrency(item.unitPrice)}</td>
                      <td className="px-4 py-2 text-right text-sm font-medium">{formatCurrency(item.totalPrice)}</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot className="bg-gray-50">
                  <tr>
                    <td colSpan={3} className="px-4 py-2 text-right text-sm font-semibold">Total:</td>
                    <td className="px-4 py-2 text-right text-sm font-bold">{formatCurrency(selectedOrder.totalAmount)}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
