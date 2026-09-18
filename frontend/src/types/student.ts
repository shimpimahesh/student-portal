export interface Student {
  id: number;
  name: string;
  dateOfBirth: string; // YYYY-MM-DD
  phone: string;
}

export interface StudentFormData {
  name: string;
  dateOfBirth: string;
  phone: string;
}