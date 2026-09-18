import type { Student, StudentFormData } from '../types/student';

const API_BASE_URL = 'https://localhost:7202/api/students';

export const studentApi = {
  async getAll(): Promise<Student[]> {
    const res = await fetch(API_BASE_URL);
    if (!res.ok) throw new Error('Failed to fetch students');
    return res.json();
  },

  async create(student: StudentFormData): Promise<Student> {
    const res = await fetch(API_BASE_URL, {
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
    const res = await fetch(`${API_BASE_URL}/${id}`, {
      method: 'DELETE',
    });
    if (!res.ok) throw new Error('Failed to delete student');
  }
};