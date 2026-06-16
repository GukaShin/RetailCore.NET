import { useEffect, useState } from 'react';
import { api } from '../api';
import { DataTable, ResultBox, useAsync } from '../components';

const emptyCreate = { name: '', description: '' };
const emptyUpdate = { name: '', description: '', isActive: true };

export default function CategoriesPage() {
  const { loading, error, result, run, setResult } = useAsync();
  const [categories, setCategories] = useState([]);
  const [selectedId, setSelectedId] = useState('');
  const [createForm, setCreateForm] = useState(emptyCreate);
  const [updateForm, setUpdateForm] = useState(emptyUpdate);

  const loadAll = () =>
    run(async () => {
      const data = await api('/api/categories');
      setCategories(data);
      return data;
    });

  useEffect(() => {
    loadAll().catch(() => {});
  }, []);

  const loadOne = (id) => {
    setSelectedId(String(id));
    run(() => api(`/api/categories/${id}`));
  };

  const selectForEdit = (cat) => {
    setSelectedId(String(cat.id));
    setUpdateForm({ name: cat.name, description: cat.description ?? '', isActive: cat.isActive });
  };

  return (
    <section className="panel">
      <h2>Categories</h2>
      <p className="hint">GET is public. Create/Update/Delete need CatalogManagement (Admin, StoreManager).</p>

      <button className="action" onClick={loadAll} disabled={loading}>
        GET /api/categories
      </button>

      <DataTable
        rows={categories}
        columns={[
          { key: 'id', label: 'ID' },
          { key: 'name', label: 'Name' },
          { key: 'description', label: 'Description' },
          { key: 'isActive', label: 'Active', render: (r) => (r.isActive ? 'Yes' : 'No') },
        ]}
        onRowClick={selectForEdit}
      />

      <div className="grid-2">
        <div>
          <h3>GET /api/categories/&#123;id&#125;</h3>
          <div className="inline">
            <div>
              <label>Category ID</label>
              <input value={selectedId} onChange={(e) => setSelectedId(e.target.value)} />
            </div>
            <button className="action secondary" disabled={loading || !selectedId} onClick={() => loadOne(selectedId)}>
              Load
            </button>
          </div>
        </div>

        <div>
          <h3>POST /api/categories</h3>
          <label>Name</label>
          <input value={createForm.name} onChange={(e) => setCreateForm({ ...createForm, name: e.target.value })} />
          <label>Description</label>
          <input
            value={createForm.description}
            onChange={(e) => setCreateForm({ ...createForm, description: e.target.value })}
          />
          <button
            className="action"
            disabled={loading}
            onClick={() =>
              run(async () => {
                const data = await api('/api/categories', { method: 'POST', body: JSON.stringify(createForm) });
                await loadAll();
                return data;
              })
            }
          >
            Create
          </button>
        </div>

        <div>
          <h3>PUT /api/categories/&#123;id&#125;</h3>
          <label>Category ID</label>
          <input value={selectedId} onChange={(e) => setSelectedId(e.target.value)} />
          <label>Name</label>
          <input value={updateForm.name} onChange={(e) => setUpdateForm({ ...updateForm, name: e.target.value })} />
          <label>Description</label>
          <input
            value={updateForm.description}
            onChange={(e) => setUpdateForm({ ...updateForm, description: e.target.value })}
          />
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
            disabled={loading || !selectedId}
            onClick={() =>
              run(async () => {
                const data = await api(`/api/categories/${selectedId}`, {
                  method: 'PUT',
                  body: JSON.stringify(updateForm),
                });
                await loadAll();
                return data;
              })
            }
          >
            Update
          </button>
          <button
            className="action danger"
            disabled={loading || !selectedId}
            onClick={() =>
              run(async () => {
                await api(`/api/categories/${selectedId}`, { method: 'DELETE' });
                setSelectedId('');
                setResult({ deleted: selectedId });
                await loadAll();
                return { message: 'Deleted' };
              })
            }
          >
            DELETE
          </button>
        </div>
      </div>

      <ResultBox error={error} result={result} />
    </section>
  );
}
