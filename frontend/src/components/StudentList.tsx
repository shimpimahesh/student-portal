import React from 'react';
import type { Student } from '../types/student';
import { Trash2, Calendar, Phone } from 'lucide-react';

interface Props {
  students: Student[];
  onDelete: (id: number) => void;
  deletingId: number | null;
}

export const StudentList: React.FC<Props> = ({ students, onDelete, deletingId }) => {
  if (students.length === 0) {
    return (
      <div className="text-center py-12 bg-white rounded-xl border border-dashed border-slate-200 text-slate-400">
        No students registered yet. Add one from the form.
      </div>
    );
  }

  return (
    <div className="overflow-hidden bg-white rounded-xl shadow-sm border border-slate-100">
      <div className="divide-y divide-slate-100">
        {students.map((student) => (
          <div key={student.id} className="p-4 flex items-center justify-between hover:bg-slate-50 transition">
            <div className="space-y-1">
              <h3 className="font-medium text-slate-800 text-sm">{student.name}</h3>
              <div className="flex flex-wrap gap-4 text-xs text-slate-500">
                <span className="flex items-center gap-1">
                  <Calendar className="w-3.5 h-3.5" />
                  {student.dateOfBirth}
                </span>
                <span className="flex items-center gap-1">
                  <Phone className="w-3.5 h-3.5" />
                  {student.phone}
                </span>
              </div>
            </div>

            <button
              onClick={() => onDelete(student.id)}
              disabled={deletingId === student.id}
              className="p-2 text-slate-400 hover:text-red-600 rounded-lg hover:bg-red-50 transition disabled:opacity-30"
              title="Delete student"
            >
              <Trash2 className="w-4 h-4" />
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};