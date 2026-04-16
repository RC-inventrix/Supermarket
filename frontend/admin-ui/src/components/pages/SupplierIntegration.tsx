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
          className="flex w-full items-center gap-1 py-0.5 text-left text-xs hover:bg-gray-50"
          style={{ paddingLeft: `${depth * 12 + 4}px` }}
          onClick={() => setExpanded((e) => !e)}
        >
          {expanded ? (
            <HiChevronDown className="h-3 w-3 text-gray-400" />
          ) : (
            <HiChevronRight className="h-3 w-3 text-gray-400" />
          )}
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
          className="flex w-full items-center gap-1 py-0.5 text-left text-xs hover:bg-gray-50"
          style={{ paddingLeft: `${depth * 12 + 4}px` }}
          onClick={() => setExpanded((e) => !e)}
        >
          {expanded ? (
            <HiChevronDown className="h-3 w-3 text-gray-400" />
          ) : (
            <HiChevronRight className="h-3 w-3 text-gray-400" />
          )}
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
      className={`flex w-full items-center gap-2 rounded py-0.5 text-left text-xs transition-colors hover:bg-blue-50 ${
        isSelected ? 'bg-blue-100 ring-1 ring-blue-400' : ''
      }`}
      style={{ paddingLeft: `${depth * 12 + 4}px` }}
      onClick={() => onSelect(path, data)}
    >
      <span className="text-gray-600">{path.split('.').pop()}:</span>
      <span className={typeof data === 'number' ? 'text-green-700' : 'text-orange-700'}>
        {truncated}
      </span>
      {isSelected && <HiCheck className="ml-auto mr-1 h-3 w-3 text-blue-600" />}
    </button>
  );
}

// ─── Field Mapping Row ────────────────────────────────────────────────────────

interface MappingRowProps {
  fieldName: string;
  fieldLabel: string;
  mappedPath: string;
  onSelectFromTree: (fieldName: string) => void;
  isSelecting: boolean;
}

function MappingRow({ fieldName, fieldLabel, mappedPath, onSelectFromTree, isSelecting }: MappingRowProps) {
  return (
    <div className="flex items-center gap-3 rounded-lg border border-gray-200 p-3">
      <div className="w-40 text-sm font-medium text-gray-700">{fieldLabel}</div>
      <div className="flex-1 rounded-md border border-dashed border-gray-300 bg-gray-50 px-3 py-2 font-mono text-xs text-gray-600">
        {mappedPath || <span className="italic text-gray-400">not mapped — click a field in the tree</span>}
      </div>
      <Button
        variant={isSelecting ? 'primary' : 'outline'}
        size="sm"
        onClick={() => onSelectFromTree(fieldName)}
      >
        {isSelecting ? 'Selecting…' : 'Pick Field'}
      </Button>
    </div>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export function SupplierIntegration() {
  const { suppliers, loading, refetch, deleteSupplier } = useSuppliers();
  const [addModalOpen, setAddModalOpen] = useState(false);
  const [mappingModalOpen, setMappingModalOpen] = useState(false);
  const [selectedSupplier, setSelectedSupplier] = useState<Supplier | null>(null);

  // JSON mapper state
  const [sampleJson, setSampleJson] = useState<Record<string, unknown> | null>(null);
  const [fetchingJson, setFetchingJson] = useState(false);
  const [savingMapping, setSavingMapping] = useState(false);
  const [activePickField, setActivePickField] = useState<string | null>(null);
  const [mapping, setMapping] = useState<FieldMapping>({
    nameField: '',
    priceField: '',
    availableQuantityField: '',
  });

  // Add supplier form
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<SupplierFormData>({ resolver: zodResolver(supplierSchema) });

  const onAddSupplier = async (data: SupplierFormData) => {
    await supplierService.create(data);
    toast.success('Supplier added successfully');
    reset();
    setAddModalOpen(false);
    refetch();
  };

  const openMappingModal = (supplier: Supplier) => {
    setSelectedSupplier(supplier);
    setSampleJson(null);
    setMapping(
      supplier.mappingConfig ?? {
        nameField: '',
        priceField: '',
        availableQuantityField: '',
      }
    );
    setMappingModalOpen(true);
  };

  const handleFetchJson = async () => {
    if (!selectedSupplier) return;
    setFetchingJson(true);
    try {
      const json = await supplierService.fetchSampleJson(
        selectedSupplier.supplierBaseUrl,
        selectedSupplier.productsEndpoint
      );
      setSampleJson(json);
      toast.success('Sample JSON fetched successfully');
    } catch {
      toast.error('Failed to fetch sample JSON. Check the supplier URL and endpoint.');
    } finally {
      setFetchingJson(false);
    }
  };

  const handleSelectPath = useCallback(
    (path: string, _value: unknown) => {
      if (!activePickField) return;
      setMapping((prev) => ({ ...prev, [activePickField]: path }));
      setActivePickField(null);
      toast.success(`Mapped "${activePickField}" to "${path}"`);
    },
    [activePickField]
  );

  const handleSaveMapping = async () => {
    if (!selectedSupplier) return;
    setSavingMapping(true);
    try {
      await supplierService.saveMapping(selectedSupplier.id, mapping);
      toast.success('Mapping saved successfully');
      setMappingModalOpen(false);
      refetch();
    } catch {
      toast.error('Failed to save mapping');
    } finally {
      setSavingMapping(false);
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
        <div>
          <p className="text-sm text-gray-500">
            Configure supplier integrations and map external JSON fields to internal schema.
          </p>
        </div>
        <Button onClick={() => setAddModalOpen(true)}>
          <HiPlus className="h-4 w-4" />
          Add Supplier
        </Button>
      </div>

      {/* Suppliers List */}
      <Card padding="none">
        <div className="border-b border-gray-200 px-6 py-4">
          <h3 className="font-semibold text-gray-900">Configured Suppliers</h3>
        </div>
        {loading ? (
          <div className="flex h-32 items-center justify-center">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent" />
          </div>
        ) : suppliers.length === 0 ? (
          <div className="py-12 text-center text-sm text-gray-500">
            No suppliers configured yet. Click "Add Supplier" to get started.
          </div>
        ) : (
          <div className="divide-y divide-gray-200">
            {suppliers.map((supplier) => (
              <div key={supplier.id} className="flex items-center justify-between px-6 py-4">
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    <span className="font-medium text-gray-900">{supplier.name}</span>
                    <Badge
                      label={supplier.isActive ? 'active' : 'inactive'}
                      className={supplier.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}
                    />
                    {supplier.mappingConfig && (
                      <Badge label="mapped" className="bg-blue-100 text-blue-800" />
                    )}
                  </div>
                  <p className="mt-0.5 text-xs text-gray-500">
                    {supplier.supplierBaseUrl}{supplier.productsEndpoint}
                  </p>
                </div>
                <div className="flex gap-2">
                  <Button variant="outline" size="sm" onClick={() => handleImport(supplier)}>
                    <HiRefresh className="h-4 w-4" />
                    Import
                  </Button>
                  <Button variant="outline" size="sm" onClick={() => openMappingModal(supplier)}>
                    Configure Mapping
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => deleteSupplier(supplier.id)}
                  >
                    <HiTrash className="h-4 w-4 text-red-500" />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}
      </Card>

      {/* Add Supplier Modal */}
      <Modal isOpen={addModalOpen} onClose={() => setAddModalOpen(false)} title="Add New Supplier">
        <form onSubmit={handleSubmit(onAddSupplier)} className="space-y-4">
          <Input
            label="Supplier Name"
            required
            {...register('name')}
            error={errors.name?.message}
          />
          <Input
            label="Base URL"
            placeholder="https://supplier.example.com"
            required
            {...register('supplierBaseUrl')}
            error={errors.supplierBaseUrl?.message}
          />
          <Input
            label="Products Endpoint"
            placeholder="/api/products"
            required
            {...register('productsEndpoint')}
            error={errors.productsEndpoint?.message}
          />
          <div className="flex justify-end gap-3 pt-2">
            <Button variant="outline" type="button" onClick={() => setAddModalOpen(false)}>
              Cancel
            </Button>
            <Button type="submit" loading={isSubmitting}>
              Add Supplier
            </Button>
          </div>
        </form>
      </Modal>

      {/* Mapping Modal */}
      <Modal
        isOpen={mappingModalOpen}
        onClose={() => setMappingModalOpen(false)}
        title={`Configure Mapping — ${selectedSupplier?.name ?? ''}`}
        size="xl"
      >
        <div className="space-y-4">
          {/* Fetch JSON */}
          <div className="flex items-center gap-3">
            <p className="flex-1 text-sm text-gray-600">
              Fetch a sample JSON payload from the supplier to visually map fields.
            </p>
            <Button variant="outline" size="sm" loading={fetchingJson} onClick={handleFetchJson}>
              <HiRefresh className="h-4 w-4" />
              Fetch Sample JSON
            </Button>
          </div>

          {sampleJson && (
            <div className="grid grid-cols-2 gap-4">
              {/* JSON Tree */}
              <div>
                <h4 className="mb-2 text-sm font-medium text-gray-700">
                  JSON Tree
                  {activePickField && (
                    <span className="ml-2 text-xs text-blue-600">
                      Click a leaf value to map to <strong>{activePickField}</strong>
                    </span>
                  )}
                </h4>
                <div className="max-h-80 overflow-y-auto rounded-lg border border-gray-200 bg-white p-2">
                  <JsonTreeNode
                    data={sampleJson}
                    path=""
                    onSelect={handleSelectPath}
                    selectedPath={null}
                  />
                </div>
              </div>

              {/* Field Mappings */}
              <div>
                <h4 className="mb-2 text-sm font-medium text-gray-700">Field Mappings</h4>
                <div className="space-y-2">
                  <MappingRow
                    fieldName="nameField"
                    fieldLabel="Product Name"
                    mappedPath={mapping.nameField}
                    onSelectFromTree={setActivePickField}
                    isSelecting={activePickField === 'nameField'}
                  />
                  <MappingRow
                    fieldName="priceField"
                    fieldLabel="Price"
                    mappedPath={mapping.priceField}
                    onSelectFromTree={setActivePickField}
                    isSelecting={activePickField === 'priceField'}
                  />
                  <MappingRow
                    fieldName="availableQuantityField"
                    fieldLabel="Available Qty"
                    mappedPath={mapping.availableQuantityField}
                    onSelectFromTree={setActivePickField}
                    isSelecting={activePickField === 'availableQuantityField'}
                  />
                </div>

                {/* JSON Preview */}
                <div className="mt-4">
                  <h4 className="mb-1 text-xs font-medium text-gray-600">Generated Config</h4>
                  <pre className="max-h-32 overflow-auto rounded-md bg-gray-900 p-3 text-xs text-green-400">
                    {JSON.stringify(mapping, null, 2)}
                  </pre>
                </div>
              </div>
            </div>
          )}

          {!sampleJson && (
            <div className="rounded-lg border-2 border-dashed border-gray-200 p-8 text-center text-sm text-gray-500">
              Fetch a sample JSON to begin visual field mapping.
            </div>
          )}

          <div className="flex justify-end gap-3 border-t border-gray-200 pt-4">
            <Button variant="outline" onClick={() => setMappingModalOpen(false)}>
              Cancel
            </Button>
            <Button loading={savingMapping} onClick={handleSaveMapping} disabled={!sampleJson}>
              <HiCheck className="h-4 w-4" />
              Save Mapping
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
