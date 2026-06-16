import { createContext, useCallback, useContext, useEffect, useState } from 'react';
import { api, clearTokens, getTokens, setTokens } from './api';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  const loadUser = useCallback(async () => {
    const { accessToken } = getTokens();
    if (!accessToken) {
      setUser(null);
      setLoading(false);
      return;
    }
    try {
      const me = await api('/api/auth/me');
      setUser(me);
    } catch {
      clearTokens();
      setUser(null);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadUser();
  }, [loadUser]);

  const login = async (email, password) => {
    const data = await api('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
    setTokens(data.accessToken, data.refreshToken);
    setUser(data.user);
    return data;
  };

  const register = async (fullName, email, password) => {
    const data = await api('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify({ fullName, email, password }),
    });
    setTokens(data.accessToken, data.refreshToken);
    setUser(data.user);
    return data;
  };

  const logout = () => {
    clearTokens();
    setUser(null);
  };

  const refreshSession = async () => {
    const { refreshToken } = getTokens();
    if (!refreshToken) throw new Error('No refresh token');
    const data = await api('/api/auth/refresh', {
      method: 'POST',
      body: JSON.stringify({ refreshToken }),
    });
    setTokens(data.accessToken, data.refreshToken);
    setUser(data.user);
    return data;
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout, refreshSession, reloadUser: loadUser }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
