import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { AppProvider } from './context/AppContext';
import { AuthProvider } from './context/AuthContext';
import { DashboardLayout } from './components/layout/DashboardLayout';
import { Dashboard } from './components/pages/Dashboard';
import { SupplierIntegration } from './components/pages/SupplierIntegration';
import { ProductManagement } from './components/pages/ProductManagement';
import { OrderManagement } from './components/pages/OrderManagement';
import { Cart } from './components/pages/Cart'; // <--- Added this import

function App() {
  return (
    <AuthProvider>
      <AppProvider>
        <BrowserRouter>
          <DashboardLayout>
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/suppliers" element={<SupplierIntegration />} />
              <Route path="/products" element={<ProductManagement />} />
              <Route path="/orders" element={<OrderManagement />} />
              <Route path="/cart" element={<Cart />} /> {/* <--- Added this line */}
            </Routes>
          </DashboardLayout>
          <ToastContainer position="top-right" autoClose={3000} />
        </BrowserRouter>
      </AppProvider>
    </AuthProvider>
  );
}

export default App;