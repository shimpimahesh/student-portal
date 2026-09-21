import type { AuthResponse, LoginData, User } from '../types/auth';

const API_ROOT_URL = import.meta.env.VITE_API_URL?.replace(/\/$/, '');
if (!API_ROOT_URL) {
  throw new Error('VITE_API_URL must be configured for the current environment');
}
const API_BASE_URL = `${API_ROOT_URL}/auth`;
const ACCESS_TOKEN_KEY = 'student-portal.access-token';
const REFRESH_TOKEN_KEY = 'student-portal.refresh-token';
const USER_KEY = 'student-portal.user';

const saveSession = (session: AuthResponse) => {
  localStorage.setItem(ACCESS_TOKEN_KEY, session.accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, session.refreshToken);
  localStorage.setItem(USER_KEY, JSON.stringify(session.user));
};

export const authApi = {
  getAccessToken: () => localStorage.getItem(ACCESS_TOKEN_KEY),

  getStoredUser: (): User | null => {
    const value = localStorage.getItem(USER_KEY);
    return value ? JSON.parse(value) as User : null;
  },

  async login(credentials: LoginData): Promise<User> {
    const res = await fetch(`${API_BASE_URL}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(credentials),
    });
    if (!res.ok) {
      const errorData = await res.json().catch(() => null);
      throw new Error(errorData?.message || 'Invalid email or password');
    }
    const session: AuthResponse = await res.json();
    saveSession(session);
    return session.user;
  },

  async refresh(): Promise<boolean> {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!refreshToken) return false;

    const res = await fetch(`${API_BASE_URL}/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    });
    if (!res.ok) {
      authApi.clearSession();
      return false;
    }
    saveSession(await res.json() as AuthResponse);
    return true;
  },

  async getCurrentUser(): Promise<User | null> {
    if (!authApi.getAccessToken()) return null;
    let res = await authApi.requestAuth('/me');
    if (res.status === 401 && await authApi.refresh()) {
      res = await authApi.requestAuth('/me');
    }
    if (!res.ok) return null;
    const currentUser: User = await res.json();
    localStorage.setItem(USER_KEY, JSON.stringify(currentUser));
    return currentUser;
  },

  async logout(): Promise<void> {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (refreshToken && authApi.getAccessToken()) {
      await fetch(`${API_BASE_URL}/logout`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${authApi.getAccessToken()}`,
        },
        body: JSON.stringify({ refreshToken }),
      }).catch(() => undefined);
    }
    authApi.clearSession();
  },

  clearSession() {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  },

  async request(path: string, init: RequestInit = {}): Promise<Response> {
    const headers = new Headers(init.headers);
    const token = authApi.getAccessToken();
    if (token) headers.set('Authorization', `Bearer ${token}`);

    let res = await fetch(`${API_ROOT_URL}${path}`, { ...init, headers });
    if (res.status === 401 && await authApi.refresh()) {
      headers.set('Authorization', `Bearer ${authApi.getAccessToken()}`);
      res = await fetch(`${API_ROOT_URL}${path}`, { ...init, headers });
    }
    return res;
  },

  async requestAuth(path: string, init: RequestInit = {}): Promise<Response> {
    const headers = new Headers(init.headers);
    const token = authApi.getAccessToken();
    if (token) headers.set('Authorization', `Bearer ${token}`);
    return fetch(`${API_BASE_URL}${path}`, { ...init, headers });
  },
};