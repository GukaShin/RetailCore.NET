import { useState } from 'react';
import { useAuth } from './AuthContext';
import AuthPage from './pages/AuthPage';
import CategoriesPage from './pages/CategoriesPage';
import ProductsPage from './pages/ProductsPage';
import InventoryPage from './pages/InventoryPage';
import ShiftsPage from './pages/ShiftsPage';
import SalesPage from './pages/SalesPage';

const TABS = [
  { id: 'auth', label: 'Auth' },
  { id: 'categories', label: 'Categories' },
  { id: 'products', label: 'Products' },
  { id: 'inventory', label: 'Inventory' },
  { id: 'shifts', label: 'Shifts' },
  { id: 'sales', label: 'Sales / POS' },
];

export default function App() {
  const { user, loading } = useAuth();
  const [tab, setTab] = useState('auth');

  const renderTab = () => {
    switch (tab) {
      case 'auth':
        return <AuthPage />;
      case 'categories':
        return <CategoriesPage />;
      case 'products':
        return <ProductsPage />;
      case 'inventory':
        return <InventoryPage />;
      case 'shifts':
        return <ShiftsPage />;
      case 'sales':
        return <SalesPage />;
      default:
        return null;
    }
  };

  return (
    <div className="app">
      <header>
        <h1>RetailCore Demo UI</h1>
        <p>Simple frontend covering all 27 API endpoints — API at localhost:5176</p>
        <div className="user-bar">
          {loading ? (
            <span>Loading session…</span>
          ) : user ? (
            <>
              <span>
                {user.fullName} · {user.role}
                {user.storeId != null && ` · Store #${user.storeId}`}
              </span>
              <span className="badge">authenticated</span>
            </>
          ) : (
            <span>Not logged in — start on Auth tab</span>
          )}
        </div>
      </header>

      <nav>
        {TABS.map((t) => (
          <button key={t.id} className={tab === t.id ? 'active' : ''} onClick={() => setTab(t.id)}>
            {t.label}
          </button>
        ))}
      </nav>

      {renderTab()}
    </div>
  );
}
