import { type ReactNode, useEffect } from 'react';
import { HiX } from 'react-icons/hi';

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  children: ReactNode;
  size?: 'sm' | 'md' | 'lg' | 'xl' | '4xl';
}

const sizeClasses = {
  sm: 'max-w-md',
  md: 'max-w-lg',
  lg: 'max-w-2xl',
  xl: 'max-w-4xl',
  '4xl': 'max-w-6xl', // Added to support the wide Supplier Onboarding Wizard
};

export function Modal({ isOpen, onClose, title, children, size = 'md' }: ModalProps) {
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6">
      {/* THE FIX: Changed from absolute to fixed. 
        This guarantees the shade covers the entire browser window 100%. 
      */}
      <div
        className="fixed inset-0 bg-gray-900/50 backdrop-blur-sm transition-opacity"
        onClick={onClose}
        aria-hidden="true"
      />

      {/* MODAL PANEL: 
        Uses flex-col and max-h-[90vh] so the header stays pinned 
        while the long form scrolls safely inside. 
      */}
      <div
        className={`relative z-10 w-full ${sizeClasses[size]} mx-auto flex max-h-[90vh] flex-col overflow-hidden rounded-xl bg-white shadow-2xl`}
        onClick={(e) => e.stopPropagation()} // Prevents clicking the white box from closing it
      >
        {/* Header - Fixed at the top */}
        {title && (
          <div className="flex shrink-0 items-center justify-between border-b border-gray-200 px-6 py-4">
            <h2 className="text-lg font-semibold text-gray-900">{title}</h2>
            <button
              onClick={onClose}
              className="rounded-md p-1 text-gray-400 hover:bg-gray-100 hover:text-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <HiX className="h-5 w-5" />
            </button>
          </div>
        )}
        
        {/* Body - Scrollable area for your long forms */}
        <div className="flex-1 overflow-y-auto px-6 py-4">
          {children}
        </div>
      </div>
    </div>
  );
}