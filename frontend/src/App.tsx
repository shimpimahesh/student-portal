import { useEffect, useState } from 'react';
import type { Student, StudentFormData } from './types/student';
import { studentApi } from './Api/studentApi';
import { StudentForm } from './components/StudentForm';
import { StudentList } from './components/StudentList';
import { Login } from './components/Login';
import { authApi } from './Api/authApi';
import type { User } from './types/auth';
import { GraduationCap, AlertCircle, RefreshCw, LogOut } from 'lucide-react';

export default function App() {
  const [user, setUser] = useState<User | null>(() => authApi.getStoredUser());
  const [authLoading, setAuthLoading] = useState(true);
  const [students, setStudents] = useState<Student[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<number | null>(null);

  const fetchStudents = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await studentApi.getAll();
      setStudents(data);
    } catch (err: any) {
      setError('Could not connect to API. Please ensure the backend is running.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    authApi.getCurrentUser().then(setUser).finally(() => setAuthLoading(false));
  }, []);

  useEffect(() => {
    if (user) fetchStudents();
  }, [user]);

  if (authLoading) return <div className="min-h-screen flex items-center justify-center text-slate-500">Checking session...</div>;
  if (!user) return <Login onLogin={async (credentials) => setUser(await authApi.login(credentials))} />;

  const handleLogout = async () => {
    await authApi.logout();
    setUser(null);
    setStudents([]);
  };

  const handleAdd = async (newStudent: StudentFormData) => {
    const created = await studentApi.create(newStudent);
    setStudents((prev) => [created, ...prev]);
  };

  const handleDelete = async (id: number) => {
    try {
      setDeletingId(id);
      await studentApi.delete(id);
      setStudents((prev) => prev.filter((s) => s.id !== id));
    } catch {
      alert('Failed to delete student');
    } finally {
      setDeletingId(null);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 py-10 px-4 sm:px-6">
      <div className="max-w-4xl mx-auto space-y-8">
        {/* Header */}
        <header className="flex items-center justify-between pb-6 border-b border-slate-200">
          <div className="flex items-center gap-3">
            <div className="p-2.5 bg-indigo-600 rounded-xl text-white">
              <GraduationCap className="w-6 h-6" />
            </div>
            <div>
              <h1 className="text-xl font-bold text-slate-900">Student Portal</h1>
              <p className="text-xs text-slate-500">Production-grade fullstack CRUD</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <div className="text-right hidden sm:block"><p className="text-sm font-medium text-slate-700">{user.displayName}</p><p className="text-xs text-slate-500">{user.email}</p></div>
            <button onClick={fetchStudents} className="p-2 text-slate-600 hover:bg-slate-200 rounded-lg transition" title="Reload data"><RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} /></button>
            <button onClick={handleLogout} className="p-2 text-slate-600 hover:bg-slate-200 rounded-lg transition" title="Sign out"><LogOut className="w-4 h-4" /></button>
          </div>
        </header>

        {/* Global Error Notice */}
        {error && (
          <div className="flex items-center gap-2 p-4 bg-amber-50 border border-amber-200 text-amber-800 rounded-xl text-sm">
            <AlertCircle className="w-5 h-5 flex-shrink-0" />
            <span>{error}</span>
          </div>
        )}

        {/* Main Content Layout */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8 items-start">
          <div className="md:col-span-1">
            <StudentForm onAddStudent={handleAdd} />
          </div>

          <div className="md:col-span-2 space-y-3">
            <div className="flex items-center justify-between">
              <h2 className="text-sm font-semibold text-slate-700 uppercase tracking-wider">
                Enrolled Students ({students.length})
              </h2>
            </div>

            {loading ? (
              <div className="p-8 text-center text-sm text-slate-400">Loading student directory...</div>
            ) : (
              <StudentList students={students} onDelete={handleDelete} deletingId={deletingId} />
            )}
          </div>
        </div>
      </div>
    </div>
  );
}