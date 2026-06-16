import { useState } from 'react';
import { api, DEFAULT_STORE_ID } from '../api';
import { useAuth } from '../AuthContext';
import { DataTable, ResultBox, useAsync } from '../components';

export default function InventoryPage() {
  const { user } = useAuth();
  const { loading, error, result, run } = useAsync();
  const storeId = user?.storeId ?? DEFAULT_STORE_ID;

  const [inventory, setInventory] = useState([]);
  const [lowStock, setLowStock] = useState([]);
  const [productStock, setProductStock] = useState([]);
  const [adjustForm, setAdjustForm] = useState({ productId: '1', quantityChange: '10', reason: 'Restock' });
  const [stockProductId, setStockProductId] = useState('1');

  const loadInventory = () =>
    run(async () => {
      const data = await api(`/api/stores/${storeId}/inventory`);
      setInventory(data);
      return data;
    });

  const loadLowStock = () =>
    run(async () => {
      const data = await api(`/api/stores/${storeId}/inventory/low-stock`);
      setLowStock(data);
      return data;
    });

  const loadProductStock = () =>
    run(async () => {
      const data = await api(`/api/products/${stockProductId}/stock`);
      setProductStock(data);
      return data;
    });

  return (
    <section className="panel">
      <h2>Inventory</h2>
      <p className="hint">
        Requires InventoryAccess (Admin, StoreManager, InventoryManager). Using store #{storeId}.
      </p>

      <button className="action" onClick={loadInventory} disabled={loading}>
        GET /api/stores/&#123;storeId&#125;/inventory
      </button>
      <button className="action secondary" onClick={loadLowStock} disabled={loading}>
        GET .../inventory/low-stock
      </button>

      <h3>Store inventory</h3>
      <DataTable
        rows={inventory}
        columns={[
          { key: 'productId', label: 'Product' },
          { key: 'productName', label: 'Name' },
          { key: 'barcode', label: 'Barcode' },
          { key: 'quantity', label: 'Qty' },
          { key: 'availableQuantity', label: 'Available' },
          { key: 'lowStockThreshold', label: 'Threshold' },
          { key: 'isLowStock', label: 'Low?', render: (r) => (r.isLowStock ? '⚠' : '') },
        ]}
        onRowClick={(r) => setAdjustForm({ ...adjustForm, productId: String(r.productId) })}
      />

      <h3>Low stock</h3>
      <DataTable
        rows={lowStock}
        columns={[
          { key: 'productName', label: 'Name' },
          { key: 'quantity', label: 'Qty' },
          { key: 'availableQuantity', label: 'Available' },
        ]}
      />

      <div className="grid-2">
        <div>
          <h3>POST /api/stores/&#123;storeId&#125;/inventory/adjust</h3>
          <label>Product ID</label>
          <input
            value={adjustForm.productId}
            onChange={(e) => setAdjustForm({ ...adjustForm, productId: e.target.value })}
          />
          <label>Quantity change (+/-)</label>
          <input
            value={adjustForm.quantityChange}
            onChange={(e) => setAdjustForm({ ...adjustForm, quantityChange: e.target.value })}
          />
          <label>Reason</label>
          <input value={adjustForm.reason} onChange={(e) => setAdjustForm({ ...adjustForm, reason: e.target.value })} />
          <button
            className="action"
            disabled={loading}
            onClick={() =>
              run(async () => {
                const body = {
                  productId: Number(adjustForm.productId),
                  quantityChange: Number(adjustForm.quantityChange),
                  reason: adjustForm.reason,
                };
                const data = await api(`/api/stores/${storeId}/inventory/adjust`, {
                  method: 'POST',
                  body: JSON.stringify(body),
                });
                await loadInventory();
                return data;
              })
            }
          >
            Adjust stock
          </button>
        </div>

        <div>
          <h3>GET /api/products/&#123;productId&#125;/stock</h3>
          <label>Product ID</label>
          <input value={stockProductId} onChange={(e) => setStockProductId(e.target.value)} />
          <button className="action secondary" disabled={loading} onClick={loadProductStock}>
            Load stock across stores
          </button>
          <DataTable
            rows={productStock}
            columns={[
              { key: 'storeId', label: 'Store' },
              { key: 'storeName', label: 'Name' },
              { key: 'quantity', label: 'Qty' },
              { key: 'availableQuantity', label: 'Available' },
            ]}
          />
        </div>
      </div>

      <ResultBox error={error} result={result} />
    </section>
  );
}
