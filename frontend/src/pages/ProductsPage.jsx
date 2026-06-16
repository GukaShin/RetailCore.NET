import { useEffect, useState } from 'react';
import { api } from '../api';
import { DataTable, ResultBox, useAsync } from '../components';

const emptyCreate = {
  name: '',
  barcode: '',
  sku: '',
  categoryId: '1',
  price: '2.50',
  costPrice: '1.00',
  vatPercent: '18',
};

export default function ProductsPage() {
  const { loading, error, result, run } = useAsync();
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [productId, setProductId] = useState('');
  const [barcode, setBarcode] = useState('4860001234567');
  const [createForm, setCreateForm] = useState(emptyCreate);
  const [updateForm, setUpdateForm] = useState({ ...emptyCreate, isActive: true });

  const loadProducts = () => {
    const params = new URLSearchParams();
    if (search) params.set('search', search);
    if (categoryId) params.set('categoryId', categoryId);
    const qs = params.toString();
    return run(async () => {
      const data = await api(`/api/products${qs ? `?${qs}` : ''}`);
      setProducts(data);
      return data;
    });
  };

  useEffect(() => {
    api('/api/categories').then(setCategories).catch(() => {});
  }, []);

  const selectProduct = (p) => {
    setProductId(String(p.id));
    setUpdateForm({
      name: p.name,
      barcode: p.barcode,
      sku: p.sku,
      categoryId: String(p.categoryId),
      price: String(p.price),
      costPrice: String(p.costPrice),
      vatPercent: String(p.vatPercent),
      isActive: p.isActive,
    });
  };

  return (
    <section className="panel">
      <h2>Products</h2>
      <p className="hint">
        Search needs CatalogManagement. Barcode lookup needs CashierOperations. Seeded barcodes: 4860001234567 (Coke),
        4860001234574, 4860002234561, 4860003234562, 4860004234563.
      </p>

      <div className="inline">
        <div>
          <label>Search</label>
          <input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="name, sku, barcode" />
        </div>
        <div>
          <label>Category filter</label>
          <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
            <option value="">All</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </div>
        <button className="action" onClick={loadProducts} disabled={loading}>
          GET /api/products
        </button>
      </div>

      <DataTable
        rows={products}
        columns={[
          { key: 'id', label: 'ID' },
          { key: 'name', label: 'Name' },
          { key: 'barcode', label: 'Barcode' },
          { key: 'sku', label: 'SKU' },
          { key: 'price', label: 'Price' },
          { key: 'categoryName', label: 'Category' },
        ]}
        onRowClick={selectProduct}
      />

      <div className="grid-2">
        <div>
          <h3>GET /api/products/&#123;id&#125;</h3>
          <label>Product ID</label>
          <input value={productId} onChange={(e) => setProductId(e.target.value)} />
          <button
            className="action secondary"
            disabled={loading || !productId}
            onClick={() => run(() => api(`/api/products/${productId}`))}
          >
            Load by ID
          </button>

          <h3>GET /api/products/barcode/&#123;barcode&#125;</h3>
          <label>Barcode</label>
          <input value={barcode} onChange={(e) => setBarcode(e.target.value)} />
          <button
            className="action secondary"
            disabled={loading || !barcode}
            onClick={() => run(() => api(`/api/products/barcode/${encodeURIComponent(barcode)}`))}
          >
            Lookup barcode
          </button>
        </div>

        <div>
          <h3>POST /api/products</h3>
          {['name', 'barcode', 'sku', 'categoryId', 'price', 'costPrice', 'vatPercent'].map((field) => (
            <div key={field}>
              <label>{field}</label>
              <input
                value={createForm[field]}
                onChange={(e) => setCreateForm({ ...createForm, [field]: e.target.value })}
              />
            </div>
          ))}
          <button
            className="action"
            disabled={loading}
            onClick={() =>
              run(async () => {
                const body = {
                  ...createForm,
                  categoryId: Number(createForm.categoryId),
                  price: Number(createForm.price),
                  costPrice: Number(createForm.costPrice),
                  vatPercent: Number(createForm.vatPercent),
                };
                const data = await api('/api/products', { method: 'POST', body: JSON.stringify(body) });
                await loadProducts();
                return data;
              })
            }
          >
            Create
          </button>
        </div>

        <div>
          <h3>PUT /api/products/&#123;id&#125; / DELETE</h3>
          <label>Product ID</label>
          <input value={productId} onChange={(e) => setProductId(e.target.value)} />
          {['name', 'barcode', 'sku', 'categoryId', 'price', 'costPrice', 'vatPercent'].map((field) => (
            <div key={field}>
              <label>{field}</label>
              <input
                value={updateForm[field]}
                onChange={(e) => setUpdateForm({ ...updateForm, [field]: e.target.value })}
              />
            </div>
          ))}
          <label>
            <input
              type="checkbox"
              checked={updateForm.isActive}
              onChange={(e) => setUpdateForm({ ...updateForm, isActive: e.target.checked })}
            />{' '}
            Active
          </label>
          <button
            className="action"
            disabled={loading || !productId}
            onClick={() =>
              run(async () => {
                const body = {
                  name: updateForm.name,
                  barcode: updateForm.barcode,
                  sku: updateForm.sku,
                  categoryId: Number(updateForm.categoryId),
                  price: Number(updateForm.price),
                  costPrice: Number(updateForm.costPrice),
                  vatPercent: Number(updateForm.vatPercent),
                  isActive: updateForm.isActive,
                };
                const data = await api(`/api/products/${productId}`, { method: 'PUT', body: JSON.stringify(body) });
                await loadProducts();
                return data;
              })
            }
          >
            Update
          </button>
          <button
            className="action danger"
            disabled={loading || !productId}
            onClick={() =>
              run(async () => {
                await api(`/api/products/${productId}`, { method: 'DELETE' });
                await loadProducts();
                return { message: 'Soft-deleted' };
              })
            }
          >
            DELETE (deactivate)
          </button>
        </div>
      </div>

      <ResultBox error={error} result={result} />
    </section>
  );
}
