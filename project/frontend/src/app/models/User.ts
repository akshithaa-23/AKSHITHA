export interface User {
  id?: number | string;
  fullName: string;
  email: string;
  role: string;
  isActive?: boolean;
  createdAt?: string | Date;
}
