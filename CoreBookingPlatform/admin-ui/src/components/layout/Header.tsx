import { HiBell, HiLogout } from 'react-icons/hi';
import { useAuth } from '../../context/AuthContext';

interface HeaderProps {
  title: string;
}

export function Header({ title }: HeaderProps) {
  const { user, logout } = useAuth();

  return (
    <header className="flex h-16 items-center justify-between border-b border-gray-200 bg-white px-6">
      <h2 className="text-xl font-semibold text-gray-900">{title}</h2>
      <div className="flex items-center gap-4">
        <button className="relative rounded-full p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-600">
          <HiBell className="h-5 w-5" />
        </button>
        <div className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-blue-600 text-sm font-medium text-white">
            {user?.name?.charAt(0) ?? 'A'}
          </div>
          <span className="text-sm font-medium text-gray-700">{user?.name}</span>
        </div>
        <button
          onClick={logout}
          className="rounded-md p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-600"
          title="Logout"
        >
          <HiLogout className="h-5 w-5" />
        </button>
      </div>
    </header>
  );
}
