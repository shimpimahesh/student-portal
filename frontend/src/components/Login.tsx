import { useState } from 'react';
import type { LoginData } from '../types/auth';
import { LockKeyhole, Loader2, LogIn, Mail } from 'lucide-react';

interface Props {
  onLogin: (credentials: LoginData) => Promise<void>;
}

export function Login({ onLogin }: Props) {
  const [credentials, setCredentials] = useState<LoginData>({ email: '', password: '' });
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await onLogin(credentials);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to sign in');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className="min-h-screen bg-slate-100 flex items-center justify-center px-4">
      <form onSubmit={handleSubmit} className="w-full max-w-md bg-white p-8 rounded-2xl shadow-sm border border-slate-200 space-y-6">
        <div>
          <div className="inline-flex p-3 bg-indigo-600 rounded-xl text-white mb-5"><LockKeyhole /></div>
          <h1 className="text-2xl font-bold text-slate-900">Welcome back</h1>
          <p className="text-sm text-slate-500 mt-1">Sign in to manage the student directory.</p>
        </div>

        {error && <div className="p-3 bg-red-50 text-red-700 text-sm rounded-lg border border-red-200">{error}</div>}

        <label className="block text-sm font-medium text-slate-700">
          Email
          <span className="relative block mt-2">
            <Mail className="absolute left-3 top-2.5 w-4 h-4 text-slate-400" />
            <input required type="email" value={credentials.email} onChange={(event) => setCredentials({ ...credentials, email: event.target.value })} className="w-full pl-9 pr-3 py-2.5 border border-slate-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" />
          </span>
        </label>

        <label className="block text-sm font-medium text-slate-700">
          Password
          <input required minLength={8} type="password" value={credentials.password} onChange={(event) => setCredentials({ ...credentials, password: event.target.value })} className="w-full mt-2 px-3 py-2.5 border border-slate-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500" />
        </label>

        <button disabled={submitting} className="w-full py-2.5 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 transition disabled:opacity-50 flex items-center justify-center gap-2">
          {submitting ? <Loader2 className="w-4 h-4 animate-spin" /> : <><LogIn className="w-4 h-4" /> Sign in</>}
        </button>
      </form>
    </main>
  );
}