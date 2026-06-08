export interface Trainer {
  id: number;
  gymId: string;
  userId?: string;
  fullName?: string;
  email?: string;
  specialization?: string;
  bio?: string;
  isActive: boolean;
  assignedMemberCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface TrainerDashboard {
  trainerId: number;
  assignedActiveMembers: number;
  assignedInactiveMembers: number;
  unassignedMembersInGym: number;
  activeDietPlans: number;
  activeWorkoutPlans: number;
}

export interface CreateTrainerRequest {
  gymId?: string;
  userId?: string;
  name?: string;
  email?: string;
  password?: string;
  specialization?: string;
  bio?: string;
}

export interface UpdateTrainerRequest {
  specialization?: string;
  bio?: string;
  isActive: boolean;
}

export interface AssignMembersRequest {
  memberIds: number[];
}
