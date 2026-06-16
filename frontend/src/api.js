const API_BASE = import.meta.env.VITE_API_BASE_URL ?? '';

let accessToken = localStorage.getItem('accessToken') ?? '';
let refreshToken = localStorage.getItem('refreshToken') ?? '';

export function getTokens() {
  return { accessToken, refreshToken };
}

export function setTokens(access, refresh) {
  accessToken = access ?? '';
  refreshToken = refresh ?? '';
  if (accessToken) localStorage.setItem('accessToken', accessToken);
  else localStorage.removeItem('accessToken');
  if (refreshToken) localStorage.setItem('refreshToken', refreshToken);
  else localStorage.removeItem('refreshToken');
}

export function clearTokens() {
  setTokens('', '');
}

async function refreshAccessToken() {
  if (!refreshToken) return false;
  const res = await fetch(`${API_BASE}/api/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken }),
  });
  if (!res.ok) return false;
  const data = await res.json();
  setTokens(data.accessToken, data.refreshToken);
  return true;
}

export async function api(path, options = {}) {
  const headers = { ...(options.headers ?? {}) };
  if (!(options.body instanceof FormData) && !headers['Content-Type']) {
    headers['Content-Type'] = 'application/json';
  }
  if (accessToken) headers.Authorization = `Bearer ${accessToken}`;

  let res = await fetch(`${API_BASE}${path}`, { ...options, headers });

  if (res.status === 401 && refreshToken && !options._retry) {
    const ok = await refreshAccessToken();
    if (ok) {
      headers.Authorization = `Bearer ${accessToken}`;
      res = await fetch(`${API_BASE}${path}`, { ...options, headers, _retry: true });
    }
  }

  const text = await res.text();
  let data = null;
  if (text) {
    try {
      data = JSON.parse(text);
    } catch {
      data = text;
    }
  }

  if (!res.ok) {
    const msg = data?.detail ?? data?.title ?? data?.message ?? res.statusText;
    const hint =
      res.status === 0 || msg === 'Failed to fetch'
        ? ' — Is the API running? Start it with: dotnet run --project src/RetailCore.Api'
        : '';
    throw new Error((typeof msg === 'string' ? msg : JSON.stringify(data)) + hint);
  }

  return data;
}

export const DEMO_USERS = [
  { email: 'admin@retailcore.local', role: 'Admin' },
  { email: 'manager@retailcore.local', role: 'StoreManager' },
  { email: 'cashier@retailcore.local', role: 'Cashier' },
  { email: 'inventory@retailcore.local', role: 'InventoryManager' },
];

export const DEFAULT_PASSWORD = 'Password123!';
export const DEFAULT_STORE_ID = 1;
export const DEFAULT_REGISTER_ID = 1;

export const PAYMENT_METHODS = ['Cash', 'Card', 'GiftCard', 'BankTransfer', 'Split'];
