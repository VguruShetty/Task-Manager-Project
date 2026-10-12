export type TaskStatus = 'Todo' | 'InProgress' | 'Completed';
export type TaskPriority = 'Low' | 'Medium' | 'High';

export interface UserDto {
  id: string;
  fullName: string;
  email: string;
}

export interface AuthResponseDto {
  token: string;
  refreshToken: string;
  tokenType: string;
  expiresAtUtc: string;
  user: UserDto;
}

export interface ProjectResponseDto {
  id: string;
  name: string;
  description?: string;
  ownerId: string;
  taskCount: number;
  createdAtUtc: string;
  updatedAtUtc?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ProjectQueryParameters {
  searchTerm?: string;
  sortBy?: string;
  isDescending?: boolean;
  pageNumber: number;
  pageSize: number;
}