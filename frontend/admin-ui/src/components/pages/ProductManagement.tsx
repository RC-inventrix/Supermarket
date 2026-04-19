import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { HiRefresh, HiSearch, HiPlus, HiShoppingCart, HiPencil, HiTrash, HiX } from 'react-icons/hi';
import { toast } from 'react-toastify';
import { Card } from '../ui/Card';
import { Button } from '../ui/Button';
import { Input } from '../ui/Input';
import { Modal } from '../ui/Modal';
import { Badge, StatusBadge } from '../ui/Badge';
import { Pagination } from '../ui/Table';
import { useProducts } from '../../hooks/useProducts';
import { productService, cartService, type CreateProductDto } from '../../services/productService';
import { supplierService } from '../../services/supplierService';
import { formatCurrency, formatDate } from '../../utils/helpers';
import type { Product, Category, Supplier } from '../../types';

interface DisplayProduct extends Product {
  isManual?: boolean;
}

const productFormSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  price: z.number().min(0, 'Price must be positive'),
  availableQuantity: z.number().min(0, 'Quantity must be positive'),
  categoryId: z.number().min(1, 'Category is required'),
  providerId: z.any().optional(), 
  newProviderName: z.string().optional(),
  description: z.string().optional(),
}).superRefine((data, ctx) => {
  const providerIdNum = Number(data.providerId);
  const hasProviderId = !isNaN(providerIdNum) && providerIdNum > 0;
  const hasNewName = !!data.newProviderName && data.newProviderName.trim().length > 0;

  if (!hasProviderId && !hasNewName) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Please select a supplier or add a new one",
      path: ["providerId"],
    });
  }
});

export function ProductManagement() {
  const { result, loading, error, filter, setFilter, refetch, deleteProduct } = useProducts();
  const [search, setSearch] = useState('');
  
  const [categories, setCategories] = useState<Category[]>([]);
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);

  // UI States
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [isNewSupplier, setIsNewSupplier] = useState(false);

  // Cart Modal States
  const [isCartModalOpen, setIsCartModalOpen] = useState(false);
  const [cartProduct, setCartProduct] = useState<DisplayProduct | null>(null);
  const [cartQty, setCartQty] = useState<number>(1);
  const [isAddingToCart, setIsAddingToCart] = useState(false);

  const { register, handleSubmit, reset, setValue, formState: { errors } } = useForm<CreateProductDto>({
    resolver: zodResolver(productFormSchema),
  });

  useEffect(() => {
    productService.getCategories().then(setCategories).catch(() => {});
    supplierService.getAll().then(setSuppliers).catch(() => {});
  }, []);

  const handleSearch = () => {
    setFilter((prev) => ({ ...prev, search, page: 1 }));
  };

  const openAddModal = () => {
    setEditingId(null);
    setIsNewSupplier(false);
    reset({ name: '', price: 0, availableQuantity: 0, description: '', providerId: NaN, newProviderName: '' });
    setIsFormModalOpen(true);
  };

  const openEditModal = (product: DisplayProduct) => {
    setEditingId(product.id);
    setIsNewSupplier(false);
    reset({
      name: product.name,
      price: product.price,
      availableQuantity: product.availableQuantity,
      categoryId: product.categoryId ?? 0,
      providerId: product.providerId ?? NaN,
      newProviderName: '',
      description: product.description ?? '',
    });
    setIsFormModalOpen(true);
  };

  const onSaveProduct = async (data: CreateProductDto) => {
    try {
      if (editingId) {
        await productService.update(editingId, data);
        toast.success('Product updated!');
      } else {
        await productService.create(data);
        toast.success('Product added manually!');
      }
      setIsFormModalOpen(false);
      refetch();
      supplierService.getAll().then(setSuppliers).catch(() => {});
    } catch {
      toast.error('Failed to save product.');
    }
  };

  const openCartModal = (product: DisplayProduct) => {
    setCartProduct(product);
    setCartQty(1);
    setIsCartModalOpen(true);
  };

  // THE FIX: Plugs into the actual cartService API call
  const confirmAddToCart = async () => {
    if (!cartProduct) return;
    if (cartQty > cartProduct.availableQuantity) {
      toast.error('Cannot add more than available stock!');
      return;
    }

    setIsAddingToCart(true);
    try {
      // Using a hardcoded dummy user ID of 1
      await cartService.addToCart(1, cartProduct.id, cartQty);
      toast.success(`Successfully added ${cartQty} kg of ${cartProduct.name} to your cart!`);
      setIsCartModalOpen(false);
    } catch {
      toast.error('Failed to add product to cart. Please try again.');
    } finally {
      setIsAddingToCart(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header Actions */}
      <div className="flex flex-wrap items-center justify-between gap-3">
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
          <Button variant="outline" onClick={refetch}>
            <HiRefresh className="h-4 w-4" />
            Refresh
          </Button>
        </div>
        <Button onClick={openAddModal}>
          <HiPlus className="mr-2 h-4 w-4" />
          Add Product Manually
        </Button>
      </div>

      {error && (
        <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">
          {error}
        </div>
      )}

      {/* Product Grid */}
      {loading ? (
         <div className="flex h-32 items-center justify-center">
           <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent" />
         </div>
      ) : (!result?.items || result.items.length === 0) ? (
        <Card className="py-12 text-center text-sm text-gray-500">
          No products found. Import from a supplier or add one manually.
        </Card>
      ) : (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {(result.items as DisplayProduct[]).map((product) => (
            <Card key={product.id} className="relative flex flex-col justify-between overflow-hidden shadow-sm hover:shadow-md transition-shadow">
              
              {product.isManual && (
                <div className="absolute top-3 right-3">
                  <Badge label="Manual Entry" className="bg-purple-100 text-purple-800" />
                </div>
              )}

              <div className="mb-4 pr-20">
                <h4 className="text-lg font-bold text-gray-900 truncate" title={product.name}>{product.name}</h4>
                <p className="mt-1 text-xs text-gray-500 line-clamp-2 min-h-[2rem]">
                  {product.description || <span className="italic text-gray-400">No description available</span>}
                </p>
              </div>

              <div className="space-y-1 mb-6 border-t border-gray-100 pt-3 text-sm">
                <div className="flex justify-between">
                  <span className="text-gray-500">Base Price (1kg)</span>
                  <span className="font-semibold text-green-700">{formatCurrency(product.price)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-500">Available Stock</span>
                  <span className={`font-medium ${product.availableQuantity > 0 ? 'text-gray-900' : 'text-red-500'}`}>
                    {product.availableQuantity} kg
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-500">Category</span>
                  <span className="text-gray-900">{product.categoryName ?? '—'}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-500">Supplier</span>
                  <span className="text-gray-900 truncate max-w-[120px] text-right" title={product.supplierName ?? ''}>{product.supplierName ?? '—'}</span>
                </div>
                <div className="flex justify-between items-center mt-2 pt-2 border-t border-gray-50">
                   <span className="text-xs text-gray-400 font-mono truncate">{product.externalProductId}</span>
                </div>
              </div>

              {/* Action Buttons */}
              <div className="mt-auto flex gap-2">
                <Button className="flex-1" variant="primary" size="sm" disabled={product.availableQuantity <= 0} onClick={() => openCartModal(product)}>
                  <HiShoppingCart className="mr-1 h-4 w-4" /> Cart
                </Button>
                {product.isManual && (
                  <Button variant="outline" size="sm" onClick={() => openEditModal(product)}>
                    <HiPencil className="h-4 w-4 text-gray-600" />
                  </Button>
                )}
                <Button variant="outline" size="sm" onClick={() => deleteProduct(product.id)}>
                  <HiTrash className="h-4 w-4 text-red-500" />
                </Button>
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* Pagination */}
      {result && result.totalPages > 1 && (
        <Card padding="none">
          <Pagination page={filter.page} totalPages={result.totalPages} onPageChange={(page) => setFilter((prev) => ({ ...prev, page }))} />
        </Card>
      )}

      {/* Add / Edit Product Modal */}
      <Modal isOpen={isFormModalOpen} onClose={() => setIsFormModalOpen(false)} title={editingId ? 'Edit Manual Product' : 'Add Product Manually'}>
        <form onSubmit={handleSubmit(onSaveProduct)} className="space-y-4">
          <Input label="Product Name" {...register('name')} error={errors.name?.message} />
          
          <div className="grid grid-cols-2 gap-4">
            <Input label="Price (per kg)" type="number" step="0.01" {...register('price', { valueAsNumber: true })} error={errors.price?.message} />
            <Input label="Available Quantity" type="number" {...register('availableQuantity', { valueAsNumber: true })} error={errors.availableQuantity?.message} />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Category</label>
              <select {...register('categoryId', { valueAsNumber: true })} className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500">
                <option value={NaN}>Select...</option>
                {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
              {errors.categoryId && <p className="mt-1 text-xs text-red-600">{errors.categoryId.message}</p>}
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Supplier</label>
              {!isNewSupplier ? (
                <div className="flex items-center gap-2">
                  <select {...register('providerId', { valueAsNumber: true })} className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500">
                    <option value={NaN}>Select...</option>
                    {suppliers.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                  </select>
                  <Button type="button" variant="outline" size="sm" className="px-2" onClick={() => { setIsNewSupplier(true); setValue('providerId', NaN as any); }} title="Add New Supplier">
                    <HiPlus className="h-4 w-4" />
                  </Button>
                </div>
              ) : (
                <div className="flex items-center gap-2">
                  <Input placeholder="Enter new supplier name" {...register('newProviderName')} className="flex-1" />
                  <Button type="button" variant="outline" size="sm" className="px-2" onClick={() => { setIsNewSupplier(false); setValue('newProviderName', ''); }} title="Cancel">
                    <HiX className="h-4 w-4" />
                  </Button>
                </div>
              )}
              {errors.providerId && <p className="mt-1 text-xs text-red-600">{errors.providerId.message}</p>}
            </div>
          </div>

          <div>
             <label className="mb-1 block text-sm font-medium text-gray-700">Description</label>
             <textarea {...register('description')} rows={3} className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:ring-1 focus:ring-blue-500" />
          </div>

          <div className="flex justify-end gap-3 pt-4">
            <Button type="button" variant="outline" onClick={() => setIsFormModalOpen(false)}>Cancel</Button>
            <Button type="submit">Save Product</Button>
          </div>
        </form>
      </Modal>

      {/* Add To Cart Workflow Modal */}
      <Modal isOpen={isCartModalOpen} onClose={() => setIsCartModalOpen(false)} title="Add to Cart">
        {cartProduct && (
          <div className="space-y-6">
            <div className="rounded-lg bg-gray-50 p-4">
              <h4 className="font-semibold text-gray-900">{cartProduct.name}</h4>
              <p className="text-sm text-gray-500">{cartProduct.supplierName}</p>
              <p className="mt-2 text-sm text-gray-700">Price: {formatCurrency(cartProduct.price)} / kg</p>
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-gray-700">Quantity (kg)</label>
              <Input 
                type="number" 
                min="1" 
                max={cartProduct.availableQuantity}
                value={cartQty}
                onChange={(e) => setCartQty(Number(e.target.value))}
              />
              <p className="mt-1 text-xs text-gray-500">Max available: {cartProduct.availableQuantity} kg</p>
            </div>

            <div className="border-t border-gray-200 pt-4 flex items-center justify-between">
              <span className="text-lg font-bold text-gray-900">Total:</span>
              <span className="text-2xl font-bold text-green-600">
                {formatCurrency(cartProduct.price * cartQty)}
              </span>
            </div>

            <div className="flex gap-3">
              <Button className="flex-1 justify-center" variant="outline" onClick={() => setIsCartModalOpen(false)} disabled={isAddingToCart}>Cancel</Button>
              <Button className="flex-1 justify-center" onClick={confirmAddToCart} loading={isAddingToCart}>Confirm Add</Button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}