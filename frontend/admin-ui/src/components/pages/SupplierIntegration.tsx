import { useState, useCallback } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'react-toastify';
import {
  HiPlus,
  HiTrash,
  HiRefresh,
  HiCheck,
  HiChevronRight,
  HiChevronDown,
  HiInformationCircle,
} from 'react-icons/hi';
import { Card } from '../ui/Card';
import { Button } from '../ui/Button';
import { Input } from '../ui/Input';
import { Modal } from '../ui/Modal';
import { Badge } from '../ui/Badge';
import { supplierService } from '../../services/supplierService';
import { useSuppliers } from '../../hooks/useSuppliers';
import { supplierSchema, type SupplierFormData } from '../../utils/validators';
import type { Supplier, FieldMapping } from '../../types';

// Mock Categories (Replace with API fetch later)
const CATEGORIES = [
  { id: 1, name: 'Meat' },
  { id: 2, name: 'Vegetables' }, // <--- Changed from 'Produce' to 'Vegetables'
  { id: 3, name: 'Spices' },
];

// ─── JSON Tree Viewer ────────────────────────────────────────────────────────
interface JsonTreeNodeProps {
  data: unknown;
  path: string;
  onSelect: (path: string, value: unknown) => void;
  selectedPath: string | null;
  depth?: number;
}

function JsonTreeNode({ data, path, onSelect, selectedPath, depth = 0 }: JsonTreeNodeProps) {
  const [expanded, setExpanded] = useState(depth < 2);

  if (data === null || data === undefined) {
    return (
      <div className="flex items-center gap-1 py-0.5 pl-4 text-xs">
        <span className="text-gray-400">{path.split('.').pop()}:</span>
        <span className="text-gray-400 italic">null</span>
      </div>
    );
  }

  if (typeof data === 'object' && !Array.isArray(data)) {
    const keys = Object.keys(data as Record<string, unknown>);
    return (
      <div>
        <button
          type="button"
          className="flex w-full items-center gap-1 py-0.5 text-left text-xs hover:bg-gray-50"
          style={{ paddingLeft: `${depth * 12 + 4}px` }}
          onClick={() => setExpanded((e) => !e)}
        >
          {expanded ? <HiChevronDown className="h-3 w-3 text-gray-400" /> : <HiChevronRight className="h-3 w-3 text-gray-400" />}
          <span className="font-medium text-purple-700">{path.split('.').pop() || 'root'}</span>
          <span className="text-gray-400">{'{'}{keys.length}{'}'}</span>
        </button>
        {expanded && keys.map((key) => (
          <JsonTreeNode
            key={key}
            data={(data as Record<string, unknown>)[key]}
            path={path ? `${path}.${key}` : key}
            onSelect={onSelect}
            selectedPath={selectedPath}
            depth={depth + 1}
          />
        ))}
      </div>
    );
  }

  if (Array.isArray(data)) {
    return (
      <div>
        <button
          type="button"
          className="flex w-full items-center gap-1 py-0.5 text-left text-xs hover:bg-gray-50"
          style={{ paddingLeft: `${depth * 12 + 4}px` }}
          onClick={() => setExpanded((e) => !e)}
        >
          {expanded ? <HiChevronDown className="h-3 w-3 text-gray-400" /> : <HiChevronRight className="h-3 w-3 text-gray-400" />}
          <span className="font-medium text-blue-700">{path.split('.').pop()}</span>
          <span className="text-gray-400">[{data.length}]</span>
        </button>
        {expanded && data.slice(0, 3).map((item, idx) => (
          <JsonTreeNode
            key={idx}
            data={item}
            path={`${path}[${idx}]`}
            onSelect={onSelect}
            selectedPath={selectedPath}
            depth={depth + 1}
          />
        ))}
      </div>
    );
  }

  const isSelected = selectedPath === path;
  const displayValue = String(data);
  const truncated = displayValue.length > 30 ? displayValue.slice(0, 30) + '…' : displayValue;

  return (
    <button
      type="button"
      className={`flex w-full items-center gap-2 rounded py-0.5 text-left text-xs transition-colors hover:bg-blue-50 ${
        isSelected ? 'bg-blue-100 ring-1 ring-blue-400' : ''
      }`}
      style={{ paddingLeft: `${depth * 12 + 4}px` }}
      onClick={() => onSelect(path, data)}
    >
      <span className="text-gray-600">{path.split('.').pop()}:</span>
      <span className={typeof data === 'number' ? 'text-green-700' : 'text-orange-700'}>{truncated}</span>
      {isSelected && <HiCheck className="ml-auto mr-1 h-3 w-3 text-blue-600" />}
    </button>
  );
}

// ─── Field Mapping Row ────────────────────────────────────────────────────────
interface MappingRowProps {
  fieldName: keyof FieldMapping;
  fieldLabel: string;
  mappedPath: string;
  onSelectFromTree: (fieldName: keyof FieldMapping) => void;
  isSelecting: boolean;
}

function MappingRow({ fieldName, fieldLabel, mappedPath, onSelectFromTree, isSelecting }: MappingRowProps) {
  return (
    <div className="flex items-center gap-3 rounded-lg border border-gray-200 p-2">
      <div className="w-40 text-xs font-medium text-gray-700">{fieldLabel}</div>
      <div className="flex-1 rounded-md border border-dashed border-gray-300 bg-gray-50 px-2 py-1 font-mono text-[10px] text-gray-600 truncate">
        {mappedPath || <span className="italic text-gray-400">not mapped</span>}
      </div>
      <Button variant={isSelecting ? 'primary' : 'outline'} size="sm" onClick={() => onSelectFromTree(fieldName)} className="text-xs py-1 px-2">
        {isSelecting ? 'Selecting…' : 'Pick'}
      </Button>
    </div>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────
export function SupplierIntegration() {
  const { suppliers, loading, refetch, deleteSupplier } = useSuppliers();
  const [addModalOpen, setAddModalOpen] = useState(false);
  const [confirmModalOpen, setConfirmModalOpen] = useState(false);
  const [pendingSupplierData, setPendingSupplierData] = useState<SupplierFormData | null>(null);

  // Form State
  const { register, handleSubmit, watch, reset, formState: { errors } } = useForm<SupplierFormData>({ resolver: zodResolver(supplierSchema) });
  const formBaseUrl = watch('supplierBaseUrl');
  const formCatalogEp = watch('catalogEndpoint');
  const formAvailabilityEp = watch('availabilityEndpoint');
  const formCheckoutEp = watch('checkoutEndpoint');

  // JSON Mapper State
  const [activeTab, setActiveTab] = useState<'catalog' | 'availability' | 'checkout'>('catalog');
  const [sampleJsons, setSampleJsons] = useState<{ catalog: any; availability: any; checkout: any }>({ catalog: null, availability: null, checkout: null });
  const [fetchingJson, setFetchingJson] = useState(false);
  const [activePickField, setActivePickField] = useState<keyof FieldMapping | null>(null);
  
  const [mapping, setMapping] = useState<FieldMapping>({
    arrayRootPath: '', idPath: '', namePath: '', pricePath: '',
    catalogQuantityPath: '', descriptionPath: '', availabilityQuantityPath: '', checkoutConfirmationPath: ''
  });

  const resetModals = () => {
    setAddModalOpen(false);
    setConfirmModalOpen(false);
    reset();
    setMapping({
      arrayRootPath: '', idPath: '', namePath: '', pricePath: '',
      catalogQuantityPath: '', descriptionPath: '', availabilityQuantityPath: '', checkoutConfirmationPath: ''
    });
    setSampleJsons({ catalog: null, availability: null, checkout: null });
  };

  const handleFetchJson = async (type: 'catalog' | 'availability' | 'checkout') => {
    const baseUrl = formBaseUrl;
    let endpoint = '';
    if (type === 'catalog') endpoint = formCatalogEp;
    if (type === 'availability') endpoint = formAvailabilityEp;
    if (type === 'checkout') endpoint = formCheckoutEp;

    if (!baseUrl || !endpoint) {
      toast.warn(`Please enter a valid Base URL and ${type} Endpoint first.`);
      return;
    }

    setFetchingJson(true);
    try {
      const json = await supplierService.fetchSampleJson(baseUrl, endpoint);
      setSampleJsons(prev => ({ ...prev, [type]: json }));
      toast.success(`Sample ${type} JSON fetched!`);
    } catch {
      toast.error(`Failed to fetch ${type} JSON. Check endpoints.`);
    } finally {
      setFetchingJson(false);
    }
  };

  const handleSelectPath = useCallback(
    (path: string) => {
      if (!activePickField) return;
      setMapping((prev) => ({ ...prev, [activePickField]: path }));
      setActivePickField(null);
      toast.success(`Mapped to "${path}"`);
    },
    [activePickField]
  );

  const onPreSubmit = (data: SupplierFormData) => {
    // Validate mapping before submitting
    if (!mapping.idPath || !mapping.namePath || !mapping.pricePath) {
      toast.error('Please map at least the Product ID, Name, and Price from the Catalog before saving.');
      return;
    }
    setPendingSupplierData(data);
    setConfirmModalOpen(true);
  };

  const onConfirmAddSupplier = async () => {
    if (!pendingSupplierData) return;
    try {
      const fullPayload = { ...pendingSupplierData, mappingConfig: mapping };
      await supplierService.create(fullPayload);
      toast.success('Supplier and mapping configured successfully!');
      resetModals();
      refetch();
    } catch {
      toast.error('Failed to create supplier.');
    }
  };

  const handleImport = async (supplier: Supplier) => {
    try {
      const result = await supplierService.importProducts(supplier.id);
      toast.success(`Imported ${result.imported} products from ${supplier.name}`);
    } catch {
      toast.error('Failed to import products');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <p className="text-sm text-gray-500">Configure supplier integrations and map external JSON fields to internal schema.</p>
        <Button onClick={() => setAddModalOpen(true)}>
          <HiPlus className="h-4 w-4" />
          Add Supplier
        </Button>
      </div>

      <Card padding="none">
        <div className="border-b border-gray-200 px-6 py-4">
          <h3 className="font-semibold text-gray-900">Configured Suppliers</h3>
        </div>
        {loading ? (
          <div className="flex h-32 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent" />
          </div>
        ) : suppliers.length === 0 ? (
          <div className="py-12 text-center text-sm text-gray-500">No suppliers configured yet.</div>
        ) : (
          <div className="divide-y divide-gray-200">
            {suppliers.map((supplier) => (
              <div key={supplier.id} className="flex items-center justify-between px-6 py-4">
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    <span className="font-medium text-gray-900">{supplier.name}</span>
                    <Badge label={supplier.isActive ? 'active' : 'inactive'} className={supplier.isActive ? 'bg-green-100' : 'bg-gray-100'} />
                  </div>
                  <p className="mt-0.5 text-xs text-gray-500">{supplier.supplierBaseUrl}</p>
                </div>
                <div className="flex gap-2">
                  <Button variant="outline" size="sm" onClick={() => handleImport(supplier)}>
                    <HiRefresh className="h-4 w-4" /> Import
                  </Button>
                  <Button variant="ghost" size="sm" onClick={() => deleteSupplier(supplier.id)}>
                    <HiTrash className="h-4 w-4 text-red-500" />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}
      </Card>

      {/* Add Supplier Wizard Modal */}
      <Modal isOpen={addModalOpen} onClose={resetModals} title="Onboard New Supplier" size="4xl">
        <form onSubmit={handleSubmit(onPreSubmit)} className="space-y-8">
          
          {/* Section 1: General Info */}
          <div>
            <h4 className="mb-4 text-lg font-semibold text-gray-900 border-b pb-2">1. General Information</h4>
            <div className="grid grid-cols-2 gap-4">
              <Input label="Supplier Name" placeholder="e.g., Fresh Valley Dairy" {...register('name')} error={errors.name?.message} />
              <div>
                <label className="mb-1 block text-sm font-medium text-gray-700">Category</label>
                <select {...register('categoryId', { valueAsNumber: true })} className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500">
                  <option value="">Select a category...</option>
                  {CATEGORIES.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
                {errors.categoryId && <p className="mt-1 text-xs text-red-600">{errors.categoryId.message}</p>}
              </div>
              <div className="col-span-2">
                <Input label="Base URL" placeholder="https://api.freshvalley.com/v1" {...register('supplierBaseUrl')} error={errors.supplierBaseUrl?.message} />
              </div>
            </div>
          </div>

          {/* Section 2: API Endpoints */}
          <div>
            <h4 className="mb-4 text-lg font-semibold text-gray-900 border-b pb-2">2. API Endpoints</h4>
            <div className="grid grid-cols-3 gap-4">
              <Input label="Catalog Endpoint" placeholder="/inventory/all" {...register('catalogEndpoint')} error={errors.catalogEndpoint?.message} />
              <Input label="Availability Endpoint" placeholder="/stock-check" {...register('availabilityEndpoint')} error={errors.availabilityEndpoint?.message} />
              <Input label="Checkout Endpoint" placeholder="/orders/create" {...register('checkoutEndpoint')} error={errors.checkoutEndpoint?.message} />
            </div>
          </div>

          {/* Section 3: Visual JSON Mapping */}
          <div>
            <div className="flex items-center justify-between border-b pb-2 mb-4">
              <h4 className="text-lg font-semibold text-gray-900">3. JSON Mapping Configuration</h4>
            </div>

            <div className="grid grid-cols-2 gap-6">
              {/* Left Side: Fetch & View JSON */}
              <div className="flex flex-col h-[400px]">
                <div className="flex gap-2 mb-2">
                  <Button type="button" size="sm" variant={activeTab === 'catalog' ? 'primary' : 'outline'} onClick={() => setActiveTab('catalog')}>Catalog</Button>
                  <Button type="button" size="sm" variant={activeTab === 'availability' ? 'primary' : 'outline'} onClick={() => setActiveTab('availability')}>Availability</Button>
                  <Button type="button" size="sm" variant={activeTab === 'checkout' ? 'primary' : 'outline'} onClick={() => setActiveTab('checkout')}>Checkout</Button>
                </div>
                
                <div className="flex items-center justify-between mb-2">
                  <span className="text-xs font-medium text-gray-600">Sample Data ({activeTab})</span>
                  <Button type="button" size="sm" variant="outline" loading={fetchingJson} onClick={() => handleFetchJson(activeTab)}>
                    <HiRefresh className="mr-1 h-3 w-3" /> Fetch
                  </Button>
                </div>

                <div className="flex-1 overflow-y-auto rounded-lg border border-gray-200 bg-white p-2">
                  {sampleJsons[activeTab] ? (
                    <JsonTreeNode data={sampleJsons[activeTab]} path="" onSelect={handleSelectPath} selectedPath={null} />
                  ) : (
                    <div className="flex h-full items-center justify-center text-sm text-gray-400 text-center px-4">
                      Fetch a sample response from the {activeTab} endpoint to start mapping.
                    </div>
                  )}
                </div>
              </div>

              {/* Right Side: Mapping Inputs */}
              <div className="h-[400px] overflow-y-auto pr-2">
                {activeTab === 'catalog' && (
                  <div className="space-y-2">
                    <h5 className="text-sm font-semibold text-gray-700">Catalog Mappings</h5>
                    <p className="text-xs text-gray-500 mb-2">Click fields in the JSON tree to populate these paths.</p>
                    <MappingRow fieldName="arrayRootPath" fieldLabel="Array Root Path" mappedPath={mapping.arrayRootPath} onSelectFromTree={setActivePickField} isSelecting={activePickField === 'arrayRootPath'} />
                    <MappingRow fieldName="idPath" fieldLabel="Product ID Path" mappedPath={mapping.idPath} onSelectFromTree={setActivePickField} isSelecting={activePickField === 'idPath'} />
                    <MappingRow fieldName="namePath" fieldLabel="Name Path" mappedPath={mapping.namePath} onSelectFromTree={setActivePickField} isSelecting={activePickField === 'namePath'} />
                    <MappingRow fieldName="pricePath" fieldLabel="Price Path" mappedPath={mapping.pricePath} onSelectFromTree={setActivePickField} isSelecting={activePickField === 'pricePath'} />
                    <MappingRow fieldName="catalogQuantityPath" fieldLabel="Quantity Path" mappedPath={mapping.catalogQuantityPath} onSelectFromTree={setActivePickField} isSelecting={activePickField === 'catalogQuantityPath'} />
                    <MappingRow fieldName="descriptionPath" fieldLabel="Description Path" mappedPath={mapping.descriptionPath} onSelectFromTree={setActivePickField} isSelecting={activePickField === 'descriptionPath'} />
                  </div>
                )}
                {activeTab === 'availability' && (
                  <div className="space-y-2">
                    <h5 className="text-sm font-semibold text-gray-700">Availability Mappings</h5>
                    <MappingRow fieldName="availabilityQuantityPath" fieldLabel="Live Stock Path" mappedPath={mapping.availabilityQuantityPath} onSelectFromTree={setActivePickField} isSelecting={activePickField === 'availabilityQuantityPath'} />
                  </div>
                )}
                {activeTab === 'checkout' && (
                  <div className="space-y-2">
                    <h5 className="text-sm font-semibold text-gray-700">Checkout Mappings</h5>
                    <MappingRow fieldName="checkoutConfirmationPath" fieldLabel="Confirmation ID Path" mappedPath={mapping.checkoutConfirmationPath} onSelectFromTree={setActivePickField} isSelecting={activePickField === 'checkoutConfirmationPath'} />
                  </div>
                )}
              </div>
            </div>
          </div>

          <div className="flex justify-end gap-3 border-t border-gray-200 pt-4">
            <Button variant="outline" type="button" onClick={resetModals}>Cancel</Button>
            <Button type="submit">Complete Setup</Button>
          </div>
        </form>
      </Modal>

      {/* Confirmation Modal */}
      <Modal isOpen={confirmModalOpen} onClose={() => setConfirmModalOpen(false)} title="Confirm Supplier Onboarding">
        <div className="flex flex-col items-center justify-center p-4 text-center">
          <HiInformationCircle className="mb-4 h-12 w-12 text-blue-500" />
          <h4 className="text-lg font-semibold text-gray-900">Save and Onboard?</h4>
          <p className="mt-2 text-sm text-gray-500">
            This will save the endpoints and mapping configuration for <strong>{pendingSupplierData?.name}</strong> to the database.
          </p>
          <div className="mt-6 flex w-full gap-3">
            <Button className="flex-1 justify-center" variant="outline" onClick={() => setConfirmModalOpen(false)}>Back to Edit</Button>
            <Button className="flex-1 justify-center" onClick={onConfirmAddSupplier}>Yes, Save Supplier</Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}