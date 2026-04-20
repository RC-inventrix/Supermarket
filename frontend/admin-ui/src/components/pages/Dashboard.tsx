import { useState, useEffect } from 'react';
import { HiShoppingBag, HiUsers, HiClipboardList } from 'react-icons/hi';
import { Card } from '../ui/Card';
import { adminService, type DashboardStats, type SystemStatus } from '../../services/adminService';

export function Dashboard() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [status, setStatus] = useState<SystemStatus | null>(null);

  useEffect(() => {
    // Fetch actual data from your database on load
    adminService.getStats().then(setStats).catch(() => {});
    
    // Ping the backend and the gateway to verify connection
    adminService.getSystemStatus().then(setStatus).catch(() => {
      // If the request completely fails, we know the backend is down
      setStatus({ backendConnected: false, gatewayConnected: false });
    });
  }, []);

  // Removed Revenue, updated with real dynamic data
  const statCards = [
    { label: 'Total Products', value: stats?.totalProducts ?? '—', icon: HiShoppingBag, color: 'text-blue-600', bg: 'bg-blue-50' },
    { label: 'Active Suppliers', value: stats?.activeSuppliers ?? '—', icon: HiUsers, color: 'text-green-600', bg: 'bg-green-50' },
    { label: 'Total Orders', value: stats?.totalOrders ?? '—', icon: HiClipboardList, color: 'text-purple-600', bg: 'bg-purple-50' },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-lg font-medium text-gray-900">Welcome to the Admin Dashboard</h3>
        <p className="mt-1 text-sm text-gray-500">
          Manage your supermarket platform — suppliers, products, and orders.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {statCards.map(({ label, value, icon: Icon, color, bg }) => (
          <Card key={label}>
            <div className="flex items-center gap-4">
              <div className={`rounded-lg ${bg} p-3`}>
                <Icon className={`h-6 w-6 ${color}`} />
              </div>
              <div>
                <p className="text-sm text-gray-500">{label}</p>
                <p className="text-2xl font-bold text-gray-900">{value}</p>
              </div>
            </div>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card>
          <h4 className="mb-3 font-semibold text-gray-900">Quick Actions</h4>
          <div className="space-y-2 text-sm text-gray-600">
            <p>• Navigate to <strong>Supplier Integration</strong> to onboard new suppliers and configure JSON mappings.</p>
            <p>• Use <strong>Products</strong> to manage inventory and import from suppliers.</p>
            <p>• Check <strong>Orders</strong> to view and update order statuses.</p>
          </div>
        </Card>
        <Card>
          <h4 className="mb-3 font-semibold text-gray-900">System Status</h4>
          <div className="space-y-4 mt-4">
            
            {/* Backend API Live Status */}
            <div className="flex items-center justify-between text-sm">
              <span className="text-gray-600 font-medium">Backend API</span>
              {status === null ? (
                <span className="rounded-full bg-gray-100 px-3 py-1 text-xs text-gray-600 animate-pulse">Checking...</span>
              ) : (
                <span className={`rounded-full px-3 py-1 text-xs font-semibold ${
                  status.backendConnected ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                }`}>
                  {status.backendConnected ? 'Connected' : 'Disconnected'}
                </span>
              )}
            </div>

            {/* Adapter Gateway Live Status */}
            <div className="flex items-center justify-between text-sm border-t pt-3">
              <span className="text-gray-600 font-medium">Adapter Gateway</span>
              {status === null ? (
                <span className="rounded-full bg-gray-100 px-3 py-1 text-xs text-gray-600 animate-pulse">Checking...</span>
              ) : (
                <span className={`rounded-full px-3 py-1 text-xs font-semibold ${
                  status.gatewayConnected ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                }`}>
                  {status.gatewayConnected ? 'Connected' : 'Disconnected'}
                </span>
              )}
            </div>

          </div>
        </Card>
      </div>
    </div>
  );
}