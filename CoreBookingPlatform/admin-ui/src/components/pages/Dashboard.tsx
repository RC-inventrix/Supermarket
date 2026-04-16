import { HiShoppingBag, HiUsers, HiClipboardList, HiTrendingUp } from 'react-icons/hi';
import { Card } from '../ui/Card';

const stats = [
  { label: 'Total Products', value: '—', icon: HiShoppingBag, color: 'text-blue-600', bg: 'bg-blue-50' },
  { label: 'Active Suppliers', value: '—', icon: HiUsers, color: 'text-green-600', bg: 'bg-green-50' },
  { label: 'Total Orders', value: '—', icon: HiClipboardList, color: 'text-purple-600', bg: 'bg-purple-50' },
  { label: 'Revenue', value: '—', icon: HiTrendingUp, color: 'text-orange-600', bg: 'bg-orange-50' },
];

export function Dashboard() {
  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-lg font-medium text-gray-900">Welcome to the Admin Dashboard</h3>
        <p className="mt-1 text-sm text-gray-500">
          Manage your supermarket platform — suppliers, products, and orders.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map(({ label, value, icon: Icon, color, bg }) => (
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
          <div className="space-y-2">
            <div className="flex items-center justify-between text-sm">
              <span className="text-gray-600">Backend API</span>
              <span className="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-600">Not connected</span>
            </div>
            <div className="flex items-center justify-between text-sm">
              <span className="text-gray-600">Adapter Gateway</span>
              <span className="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-600">Not connected</span>
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
}
