export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  refreshTokenExpiresAt: string;
  userId: string;
  fullName: string;
  email: string;
  gymId?: string | null;
  gymName?: string | null;
  sessionId: string;
  tokenVersion: number;
  roles: string[];
  permissions: string[];
  mustChangePassword?: boolean;
}

export interface AuthUser {
  userId: string;
  name: string;
  email: string;
  gymId?: string | null;
  roles: string[];
  permissions: string[];
  expiresAt: string;
  mustChangePassword?: boolean;
}

export interface SessionPermissions {
  userId: string;
  fullName: string;
  email: string;
  gymId?: string | null;
  gymName?: string | null;
  roles: string[];
  permissions: string[];
  refreshedAt: string;
}
