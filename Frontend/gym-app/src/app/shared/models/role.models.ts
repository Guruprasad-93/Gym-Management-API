export interface Role {
  id: number;
  roleName: string;
  description?: string;
  isSystemRole: boolean;
  createdDate: string;
}

export interface CreateRoleRequest {
  roleName: string;
  description?: string;
}

export interface UpdateRoleRequest {
  roleName: string;
  description?: string;
}

export interface Privilege {
  id: number;
  privilegeName: string;
  description?: string;
  category: string;
  createdDate: string;
}

export interface CreatePrivilegeRequest {
  privilegeName: string;
  description?: string;
  category: string;
}

export interface RolePermissionMatrix {
  roles: { roleId: number; roleName: string }[];
  privileges: { privilegeId: number; privilegeName: string; category: string }[];
  assignments: { roleId: number; privilegeId: number; assigned: boolean }[];
}
