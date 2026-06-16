import { useEffect, useState } from 'react';
import { api, DEFAULT_REGISTER_ID, DEFAULT_STORE_ID, PAYMENT_METHODS } from '../api';
import { useAuth } from '../AuthContext';
import { DataTable, ResultBox, useAsync } from '../components';

function newIdempotencyKey() {
  return crypto.randomUUID();
}

export default function SalesPage() {
  const { user } = useAuth();
  const { loading, error, result, run } = useAsync();
  const storeId = user?.storeId ?? DEFAULT_STORE_ID;

  const [sales, setSales] = useState([]);
  const [currentShift, setCurrentShift] = useState(null);
  const [saleId, setSaleId] = useState('');
  const [receipt, setReceipt] = useState(null);
  const [barcodeInput, setBarcodeInput] = useState('');
  const [cart, setCart] = useState([]);
  const [paymentMethod, setPaymentMethod] = useState('Cash');
  const [paymentAmount, setPaymentAmount] = useState('');
  const [customerId, setCustomerId] = useState('');

  const cartTotal = cart.reduce((sum, line) => sum + line.price * line.quantity, 0);

  useEffect(() => {
    if (user) {
      api('/api/shifts/my-current')
        .then(setCurrentShift)
        .catch(() => setCurrentShift(null));
    }
  }, [user]);

  const loadSales = () =>
    run(async () => {
      const data = await api(`/api/sales?storeId=${storeId}`);
      setSales(data);
      return data;
    });

  const addByBarcode = async () => {
    if (!barcodeInput.trim()) return;
    try {
      const product = await api(`/api/products/barcode/${encodeURIComponent(barcodeInput.trim())}`);
      setCart((prev) => {
        const existing = prev.find((x) => x.productId === product.id);
        if (existing) {
          return prev.map((x) => (x.productId === product.id ? { ...x, quantity: x.quantity + 1 } : x));
        }
        return [
          ...prev,
          {
            productId: product.id,
            name: product.name,
            barcode: product.barcode,
            price: product.price,
            quantity: 1,
          },
        ];
      });
      setBarcodeInput('');
    } catch (e) {
      run(() => Promise.reject(e));
    }
  };

  const updateQty = (productId, qty) => {
    setCart((prev) =>
      prev
        .map((x) => (x.productId === productId ? { ...x, quantity: Math.max(0, qty) } : x))
        .filter((x) => x.quantity > 0),
    );
  };

  const checkout = () =>
    run(async () => {
      if (!currentShift) throw new Error('Open a shift first (Shifts tab)');
      const amount = paymentAmount ? Number(paymentAmount) : cartTotal;
      const body = {
        storeId,
        shiftId: currentShift.id,
        cashRegisterId: currentShift.cashRegisterId ?? DEFAULT_REGISTER_ID,
        customerId: customerId ? Number(customerId) : null,
        items: cart.map((x) => ({ productId: x.productId, quantity: x.quantity })),
        payments: [{ paymentMethod, amount }],
        idempotencyKey: newIdempotencyKey(),
      };
      const data = await api('/api/sales/checkout', { method: 'POST', body: JSON.stringify(body) });
      setSaleId(String(data.saleId));
      setCart([]);
      setPaymentAmount('');
      const receiptData = await api(`/api/sales/${data.saleId}/receipt`);
      setReceipt(receiptData);
      return data;
    });

  const loadSaleDetail = () => run(() => api(`/api/sales/${saleId}`));

  const loadReceipt = () =>
    run(async () => {
      const data = await api(`/api/sales/${saleId}/receipt`);
      setReceipt(data);
      return data;
    });

  return (
    <section className="panel">
      <h2>Sales / POS Checkout</h2>
      <p className="hint">
        Full flow: login as cashier → open shift → scan barcodes → checkout → view receipt → close shift. Store #
        {storeId}.
      </p>

      {currentShift ? (
        <p>
          Active shift <strong>#{currentShift.id}</strong> · register #{currentShift.cashRegisterId}
        </p>
      ) : (
        <p className="error">No open shift — go to Shifts tab and open one first.</p>
      )}

      <h3>POS — POST /api/sales/checkout</h3>
      <div className="inline">
        <div>
          <label>Scan barcode</label>
          <input
            value={barcodeInput}
            onChange={(e) => setBarcodeInput(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && addByBarcode()}
            placeholder="4860001234567"
          />
        </div>
        <button className="action" onClick={addByBarcode} disabled={loading}>
          Add to cart (barcode lookup)
        </button>
      </div>

      {cart.length > 0 && (
        <table className="cart-table">
          <thead>
            <tr>
              <th>Product</th>
              <th>Price</th>
              <th>Qty</th>
              <th>Line</th>
            </tr>
          </thead>
          <tbody>
            {cart.map((line) => (
              <tr key={line.productId}>
                <td>{line.name}</td>
                <td>{line.price.toFixed(2)}</td>
                <td>
                  <input
                    type="number"
                    min="1"
                    value={line.quantity}
                    onChange={(e) => updateQty(line.productId, Number(e.target.value))}
                  />
                </td>
                <td>{(line.price * line.quantity).toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      <p>
        <strong>Cart total: {cartTotal.toFixed(2)}</strong>
      </p>

      <div className="grid-2">
        <div>
          <label>Payment method</label>
          <select value={paymentMethod} onChange={(e) => setPaymentMethod(e.target.value)}>
            {PAYMENT_METHODS.map((m) => (
              <option key={m} value={m}>
                {m}
              </option>
            ))}
          </select>
          <label>Payment amount (blank = cart total)</label>
          <input value={paymentAmount} onChange={(e) => setPaymentAmount(e.target.value)} placeholder={cartTotal.toFixed(2)} />
          <label>Customer ID (optional, no CRUD API)</label>
          <input value={customerId} onChange={(e) => setCustomerId(e.target.value)} placeholder="optional" />
          <button className="action" onClick={checkout} disabled={loading || !cart.length || !currentShift}>
            Checkout
          </button>
        </div>

        <div>
          <h3>Sales history — GET /api/sales</h3>
          <button className="action secondary" onClick={loadSales} disabled={loading}>
            Load sales (Management)
          </button>
          <DataTable
            rows={sales}
            columns={[
              { key: 'id', label: 'ID' },
              { key: 'receiptNumber', label: 'Receipt #' },
              { key: 'totalAmount', label: 'Total' },
              { key: 'paymentStatus', label: 'Payment' },
              { key: 'createdAt', label: 'When', render: (r) => new Date(r.createdAt).toLocaleString() },
            ]}
            onRowClick={(r) => setSaleId(String(r.id))}
          />
        </div>
      </div>

      <h3>Sale detail & receipt</h3>
      <div className="inline">
        <div>
          <label>Sale ID</label>
          <input value={saleId} onChange={(e) => setSaleId(e.target.value)} />
        </div>
        <button className="action secondary" disabled={loading || !saleId} onClick={loadSaleDetail}>
          GET /api/sales/&#123;id&#125;
        </button>
        <button className="action secondary" disabled={loading || !saleId} onClick={loadReceipt}>
          GET /api/sales/&#123;id&#125;/receipt
        </button>
      </div>

      {receipt?.receiptText && <div className="receipt">{receipt.receiptText}</div>}

      <ResultBox error={error} result={result} />
    </section>
  );
}
