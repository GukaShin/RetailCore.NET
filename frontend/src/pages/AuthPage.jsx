import { useState } from 'react';
import { api, DEFAULT_PASSWORD, DEMO_USERS } from '../api';
import { useAuth } from '../AuthContext';
import { ResultBox, useAsync } from '../components';

export default function AuthPage() {
  const { user, login, register, logout, refreshSession } = useAuth();
  const { loading, error, result, run } = useAsync();

  const [loginForm, setLoginForm] = useState({ email: 'cashier@retailcore.local', password: DEFAULT_PASSWORD });
  const [registerForm, setRegisterForm] = useState({ fullName: '', email: '', password: '' });

  return (
    <section className="panel">
      <h2>Authentication</h2>
      <p className="hint">
        Demo users (password: <strong>{DEFAULT_PASSWORD}</strong>): admin, manager, cashier, inventory @retailcore.local
      </p>

      {user ? (
        <>
          <p>
            Logged in as <strong>{user.fullName}</strong> <span className="badge">{user.role}</span>
            {user.storeId != null && <> · Store #{user.storeId}</>}
          </p>
          <button className="action secondary" onClick={() => run(() => api('/api/auth/me'))} disabled={loading}>
            GET /api/auth/me
          </button>
          <button className="action secondary" onClick={() => run(() => refreshSession())} disabled={loading}>
            POST /api/auth/refresh
          </button>
          <button className="action danger" onClick={logout}>
            Logout (clear tokens)
          </button>
        </>
      ) : (
        <div className="grid-2">
          <div>
            <h3>Login — POST /api/auth/login</h3>
            <div className="quick-login">
              {DEMO_USERS.map((u) => (
                <button
                  key={u.email}
                  type="button"
                  onClick={() => setLoginForm({ email: u.email, password: DEFAULT_PASSWORD })}
                >
                  {u.role}
                </button>
              ))}
            </div>
            <label>Email</label>
            <input
              value={loginForm.email}
              onChange={(e) => setLoginForm({ ...loginForm, email: e.target.value })}
            />
            <label>Password</label>
            <input
              type="password"
              value={loginForm.password}
              onChange={(e) => setLoginForm({ ...loginForm, password: e.target.value })}
            />
            <button
              className="action"
              disabled={loading}
              onClick={() => run(() => login(loginForm.email, loginForm.password))}
            >
              Login
            </button>
          </div>

          <div>
            <h3>Register — POST /api/auth/register</h3>
            <p className="hint">Creates a Cashier account (no store assigned).</p>
            <label>Full name</label>
            <input
              value={registerForm.fullName}
              onChange={(e) => setRegisterForm({ ...registerForm, fullName: e.target.value })}
            />
            <label>Email</label>
            <input
              value={registerForm.email}
              onChange={(e) => setRegisterForm({ ...registerForm, email: e.target.value })}
            />
            <label>Password</label>
            <input
              type="password"
              value={registerForm.password}
              onChange={(e) => setRegisterForm({ ...registerForm, password: e.target.value })}
            />
            <button
              className="action"
              disabled={loading}
              onClick={() => run(() => register(registerForm.fullName, registerForm.email, registerForm.password))}
            >
              Register
            </button>
          </div>
        </div>
      )}

      <ResultBox error={error} result={result} />
    </section>
  );
}
