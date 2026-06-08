export interface GymAdmin {
  userId: string;
  gymId: string;
  gymName: string;
  name: string;
  email: string;
  isActive: boolean;
  mustChangePassword: boolean;
  createdDate: string;
}

export interface CreateGymAdminRequest {
  gymId: string;
  name: string;
  email: string;
  password?: string;
  generateTemporaryPassword: boolean;
}

export interface UpdateGymAdminRequest {
  name: string;
  email: string;
  gymId: string;
}

export interface CreateGymAdminResult {
  admin: GymAdmin;
  temporaryPassword?: string;
  message: string;
}

export interface ResendTemporaryPasswordResult {
  userId: string;
  email: string;
  temporaryPassword: string;
  message: string;
}
