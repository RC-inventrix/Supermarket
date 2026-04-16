import { useState, useMemo } from 'react';
import { HiTrash, HiCheckCircle, HiShoppingCart, HiInformationCircle } from 'react-icons/hi';
import { Card } from '../ui/Card';
import { Button } from '../ui/Button';
import { Modal } from '../ui/Modal';
import { formatCurrency } from '../../utils/helpers';
import { toast } from 'react-toastify';

// Mock data for the UI implementation
interface CartItem {
  id: number;
  name: string;
  price: number;
  quantity: number;
  supplier: string;
}

const mockInitialCart: CartItem[] = [
  { id: 1, name: 'Premium Beef Steak', price: 25.99, quantity: 2, supplier: 'Meat Supplier' },
  { id: 2, name: 'Organic Carrots', price: 4.50, quantity: 5, supplier: 'Veggie API' },
  { id: 3, name: 'Black Peppercorns', price: 3.20, quantity: 1, supplier: 'Spice API' },
];

export function Cart() {
  const [cartItems, setCartItems] = useState<CartItem[]>(mockInitialCart);
  const [selectedIds, setSelectedIds] = useState<Set<number>>(new Set(mockInitialCart.map(i => i.id)));
  
  // Modal states
  const [isConfirmOpen, setIsConfirmOpen] = useState(false);
  const [isSuccessOpen, setIsSuccessOpen] = useState(false);
  const [isPlacingOrder, setIsPlacingOrder] = useState(false);

  // Calculations
  const selectedItems = cartItems.filter(item => selectedIds.has(item.id));
  const totalAmount = useMemo(() => {
    return selectedItems.reduce((sum, item) => sum + item.price * item.quantity, 0);
  }, [selectedItems]);

  const allSelected = cartItems.length > 0 && selectedIds.size === cartItems.length;

  // Handlers
  const toggleSelectAll = () => {
    if (allSelected) {
      setSelectedIds(new Set());
    } else {
      setSelectedIds(new Set(cartItems.map(i => i.id)));
    }
  };

  const toggleSelect = (id: number) => {
    const next = new Set(selectedIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    setSelectedIds(next);
  };

  const removeItem = (id: number) => {
    setCartItems(prev => prev.filter(item => item.id !== id));
    const nextSelected = new Set(selectedIds);
    nextSelected.delete(id);
    setSelectedIds(nextSelected);
    toast.info('Item removed from cart');
  };

  const clearCart = () => {
    setCartItems([]);
    setSelectedIds(new Set());
    toast.info('Cart cleared');
  };

  const handlePlaceOrder = async () => {
    setIsPlacingOrder(true);
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1500));
    
    // Remove ordered items from cart
    setCartItems(prev => prev.filter(item => !selectedIds.has(item.id)));
    setSelectedIds(new Set());
    
    setIsPlacingOrder(false);
    setIsConfirmOpen(false);
    setIsSuccessOpen(true); // Show beautiful success modal
  };

  if (cartItems.length === 0 && !isSuccessOpen) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-center">
        <div className="rounded-full bg-gray-100 p-6">
          <HiShoppingCart className="h-12 w-12 text-gray-400" />
        </div>
        <h3 className="mt-4 text-lg font-medium text-gray-900">Your cart is empty</h3>
        <p className="mt-1 text-sm text-gray-500">Add products to create a manual order.</p>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
      {/* Left Column: Cart Items */}
      <div className="space-y-4 lg:col-span-2">
        <div className="flex items-center justify-between">
          <label className="flex items-center gap-2 cursor-pointer">
            <input
              type="checkbox"
              checked={allSelected}
              onChange={toggleSelectAll}
              className="h-5 w-5 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
            <span className="text-sm font-medium text-gray-700">Select All</span>
          </label>
          <Button variant="ghost" size="sm" onClick={clearCart} className="text-red-600 hover:text-red-700">
            <HiTrash className="mr-1 h-4 w-4" />
            Delete All
          </Button>
        </div>

        {cartItems.map((item) => (
          <Card key={item.id} padding="none">
            <div className="flex items-center gap-4 p-4">
              <input
                type="checkbox"
                checked={selectedIds.has(item.id)}
                onChange={() => toggleSelect(item.id)}
                className="h-5 w-5 rounded border-gray-300 text-blue-600 focus:ring-blue-500 cursor-pointer"
              />
              <div className="flex h-16 w-16 items-center justify-center rounded-lg bg-blue-50 text-blue-600">
                <HiShoppingCart className="h-8 w-8 opacity-50" />
              </div>
              <div className="flex-1">
                <h4 className="font-semibold text-gray-900">{item.name}</h4>
                <p className="text-sm text-gray-500">Supplier: {item.supplier}</p>
                <div className="mt-1 flex items-center gap-4 text-sm">
                  <span className="font-medium text-gray-900">{formatCurrency(item.price)}</span>
                  <span className="text-gray-500">Qty: {item.quantity}</span>
                </div>
              </div>
              <div className="text-right">
                <p className="font-bold text-gray-900">{formatCurrency(item.price * item.quantity)}</p>
                <button
                  onClick={() => removeItem(item.id)}
                  className="mt-2 text-sm text-red-500 hover:text-red-700"
                >
                  <HiTrash className="h-5 w-5" />
                </button>
              </div>
            </div>
          </Card>
        ))}
      </div>

      {/* Right Column: Order Summary */}
      <div className="lg:col-span-1">
        <Card>
          <h3 className="mb-4 text-lg font-semibold text-gray-900">Order Summary</h3>
          <div className="space-y-3 text-sm">
            <div className="flex justify-between text-gray-600">
              <span>Selected Items</span>
              <span>{selectedItems.length}</span>
            </div>
            <div className="flex justify-between text-gray-600">
              <span>Subtotal</span>
              <span>{formatCurrency(totalAmount)}</span>
            </div>
            <div className="my-2 border-t border-gray-200"></div>
            <div className="flex justify-between text-base font-bold text-gray-900">
              <span>Total</span>
              <span>{formatCurrency(totalAmount)}</span>
            </div>
          </div>
          <div className="mt-6">
            <Button
              className="w-full justify-center"
              disabled={selectedItems.length === 0}
              onClick={() => setIsConfirmOpen(true)}
            >
              Place Order
            </Button>
          </div>
        </Card>
      </div>

      {/* Confirmation Modal */}
      <Modal isOpen={isConfirmOpen} onClose={() => !isPlacingOrder && setIsConfirmOpen(false)} title="Confirm Order">
        <div className="flex flex-col items-center justify-center p-4 text-center">
          <HiInformationCircle className="mb-4 h-12 w-12 text-blue-500" />
          <h4 className="text-lg font-semibold text-gray-900">Ready to place this order?</h4>
          <p className="mt-2 text-sm text-gray-500">
            You are about to place an order for {selectedItems.length} items totaling <strong>{formatCurrency(totalAmount)}</strong>.
          </p>
          <div className="mt-6 flex w-full gap-3">
            <Button className="flex-1 justify-center" variant="outline" onClick={() => setIsConfirmOpen(false)} disabled={isPlacingOrder}>
              Cancel
            </Button>
            <Button className="flex-1 justify-center" onClick={handlePlaceOrder} loading={isPlacingOrder}>
              Confirm Order
            </Button>
          </div>
        </div>
      </Modal>

      {/* Success Modal */}
      <Modal isOpen={isSuccessOpen} onClose={() => setIsSuccessOpen(false)} title="">
        <div className="flex flex-col items-center justify-center py-6 text-center">
          <div className="mb-4 rounded-full bg-green-100 p-4">
            <HiCheckCircle className="h-16 w-16 text-green-600" />
          </div>
          <h2 className="text-2xl font-bold text-gray-900">Order Placed Successfully!</h2>
          <p className="mt-2 text-gray-500">
            The order has been routed to the respective suppliers via the Adapter Gateway.
          </p>
          <Button className="mt-8 px-8" onClick={() => setIsSuccessOpen(false)}>
            Continue
          </Button>
        </div>
      </Modal>
    </div>
  );
}