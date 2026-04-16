import { NavLink } from 'react-router-dom';
import {
  HiHome,
  HiUsers,
  HiShoppingBag,
  HiClipboardList,
} from 'react-icons/hi';

const navItems = [
  { to: '/', label: 'Dashboard', icon: HiHome, exact: true },
  { to: '/suppliers', label: 'Supplier Integration', icon: HiUsers, exact: false },
  { to: '/products', label: 'Products', icon: HiShoppingBag, exact: false },
  { to: '/orders', label: 'Orders', icon: HiClipboardList, exact: false },
];

export function Navigation() {
  return (
    <nav className="mt-6 space-y-1 px-3">
      {navItems.map(({ to, label, icon: Icon, exact }) => (
        <NavLink
          key={to}
          to={to}
          end={exact}
          className={({ isActive }) =>
            `flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
              isActive
                ? 'bg-blue-50 text-blue-700'
                : 'text-gray-700 hover:bg-gray-100 hover:text-gray-900'
            }`
          }
        >
          <Icon className="h-5 w-5 flex-shrink-0" />
          {label}
        </NavLink>
      ))}
    </nav>
  );
}
