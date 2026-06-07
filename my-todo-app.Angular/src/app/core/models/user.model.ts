export interface User {
  id: string;
  firstName: string;
  lastName: string | null;
  email: string;
  passwordHash?: string;
  isDeleted: boolean;
  updatedBy?: string | null;
  lastUpdatedDate: string | null;
}

export interface UpdateUserDto {
  id: string;
  firstName: string;
  lastName: string | null;
  email: string;
  passwordHash: string;
  isDeleted: boolean;
  updatedBy?: string | null;
  lastUpdatedDate?: string | null;
}
