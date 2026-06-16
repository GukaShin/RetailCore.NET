import { useEffect, useState } from 'react';
import { api, DEFAULT_REGISTER_ID, DEFAULT_STORE_ID } from '../api';
import { useAuth } from '../AuthContext';
import { DataTable, ResultBox, useAsync } from '../components';

export default function ShiftsPage() {
  const { user } = useAuth();
  const { loading, error, result, run } = useAsync();
  const storeId = user?.storeId ?? DEFAULT_STORE_ID;

  const [currentShift, setCurrentShift] = useState(null);
  const [storeShifts, setStoreShifts] = useState([]);
  const [shiftId, setShiftId] = useState('');
  const [openForm, setOpenForm] = useState({ cashRegisterId: String(DEFAULT_REGISTER_ID), openingCashAmount: '100' });
  const [closeForm, setCloseForm] = useState({ actualCashAmount: '100' });

  const loadCurrent = () =>
    run(async () => {
      const data = await api('/api/shifts/my-current');
      setCurrentShift(data);
      if (data?.id) setShiftId(String(data.id));
      return data;
    });

  const loadStoreShifts = () =>
    run(async () => {
      const data = await api(`/api/stores/${storeId}/shifts`);
      setStoreShifts(data);
      return data;
    });

  useEffect(() => {
    if (user) loadCurrent().catch(() => {});
  }, [user]);

  return (
    <section className="panel">
      <h2>Shifts</h2>
      <p className="hint">
        Open/close needs CashierOperations. Store history needs Management. Seeded register ID: {DEFAULT_REGISTER_ID}{' '}
        (REG-01).
      </p>

      {currentShift ? (
        <p>
          Current shift: <strong>#{currentShift.id}</strong> · {currentShift.status} · opened{' '}
          {new Date(currentShift.openedAt).toLocaleString()}
        </p>
      ) : (
        <p className="hint">No open shift for current user.</p>
      )}

      <button className="action" onClick={loadCurrent} disabled={loading}>
        GET /api/shifts/my-current
      </button>
      <button className="action secondary" onClick={loadStoreShifts} disabled={loading}>
        GET /api/stores/&#123;storeId&#125;/shifts
      </button>

      <h3>Store shift history</h3>
      <DataTable
        rows={storeShifts}
        columns={[
          { key: 'id', label: 'ID' },
          { key: 'cashierId', label: 'Cashier' },
          { key: 'status', label: 'Status' },
          { key: 'openingCashAmount', label: 'Opening' },
          { key: 'actualCashAmount', label: 'Actual' },
          { key: 'openedAt', label: 'Opened', render: (r) => new Date(r.openedAt).toLocaleString() },
        ]}
        onRowClick={(r) => setShiftId(String(r.id))}
      />

      <div className="grid-2">
        <div>
          <h3>POST /api/shifts/open</h3>
          <label>Cash register ID</label>
          <input
            value={openForm.cashRegisterId}
            onChange={(e) => setOpenForm({ ...openForm, cashRegisterId: e.target.value })}
          />
          <label>Opening cash amount</label>
          <input
            value={openForm.openingCashAmount}
            onChange={(e) => setOpenForm({ ...openForm, openingCashAmount: e.target.value })}
          />
          <button
            className="action"
            disabled={loading}
            onClick={() =>
              run(async () => {
                const body = {
                  cashRegisterId: Number(openForm.cashRegisterId),
                  openingCashAmount: Number(openForm.openingCashAmount),
                };
                const data = await api('/api/shifts/open', { method: 'POST', body: JSON.stringify(body) });
                setCurrentShift(data);
                setShiftId(String(data.id));
                return data;
              })
            }
          >
            Open shift
          </button>
        </div>

        <div>
          <h3>POST /api/shifts/&#123;id&#125;/close</h3>
          <label>Shift ID</label>
          <input value={shiftId} onChange={(e) => setShiftId(e.target.value)} />
          <label>Actual cash in drawer</label>
          <input
            value={closeForm.actualCashAmount}
            onChange={(e) => setCloseForm({ ...closeForm, actualCashAmount: e.target.value })}
          />
          <button
            className="action"
            disabled={loading || !shiftId}
            onClick={() =>
              run(async () => {
                const body = { actualCashAmount: Number(closeForm.actualCashAmount) };
                const data = await api(`/api/shifts/${shiftId}/close`, { method: 'POST', body: JSON.stringify(body) });
                setCurrentShift(null);
                return data;
              })
            }
          >
            Close shift
          </button>

          <h3>GET /api/shifts/&#123;id&#125;</h3>
          <button
            className="action secondary"
            disabled={loading || !shiftId}
            onClick={() => run(() => api(`/api/shifts/${shiftId}`))}
          >
            Load shift
          </button>
        </div>
      </div>

      <ResultBox error={error} result={result} />
    </section>
  );
}
