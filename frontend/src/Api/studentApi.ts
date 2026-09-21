import type { Student, StudentFormData } from '../types/student';
import { authApi } from './authApi';

export const studentApi = {
  async getAll(): Promise<Student[]> {
    const res = await authApi.request('/students');
    if (!res.ok) throw new Error('Failed to fetch students');
    return res.json();
  },

  async create(student: StudentFormData): Promise<Student> {
    const res = await authApi.request('/students', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(student),
    });
    if (!res.ok) {
      const errorData = await res.json().catch(() => null);
      throw new Error(errorData?.title || 'Validation error occurred');
    }
    return res.json();
  },

  async delete(id: number): Promise<void> {
    const res = await authApi.request(`/students/${id}`, {
      method: 'DELETE',
    });
    if (!res.ok) throw new Error('Failed to delete student');
  }
};