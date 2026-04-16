import { Navigation } from './Navigation';
import { HiShoppingCart } from 'react-icons/hi';

export function Sidebar() {
  return (
    <aside className="flex h-full w-64 flex-col border-r border-gray-200 bg-white">
      <div className="flex h-16 items-center gap-2 border-b border-gray-200 px-4">
        <HiShoppingCart className="h-8 w-8 text-blue-600" />
        <div>
          <h1 className="text-sm font-bold text-gray-900">Supermarket</h1>
          <p className="text-xs text-gray-500">Admin Dashboard</p>
        </div>
      </div>
      <div className="flex-1 overflow-y-auto py-2">
        <Navigation />
      </div>
    </aside>
  );
}
