export const FileCategories = {
  GymLogo: 'GymLogo',
  MemberProfilePhoto: 'MemberProfilePhoto',
  TrainerProfilePhoto: 'TrainerProfilePhoto',
  MemberProgressPhoto: 'MemberProgressPhoto',
  DietAttachment: 'DietAttachment',
  WorkoutAttachment: 'WorkoutAttachment',
} as const;

export type FileCategory = (typeof FileCategories)[keyof typeof FileCategories];

export interface StoredFile {
  fileId: number;
  gymId: string;
  fileCategory: string;
  storageProvider: string;
  publicUrl: string;
  originalFileName: string;
  contentType: string;
  fileSizeBytes: number;
  width?: number | null;
  height?: number | null;
  uploadedByUserId?: string | null;
  createdAt: string;
}

export interface MemberFile {
  memberFileId: number;
  memberId: number;
  fileId: number;
  fileCategory: string;
  dietPlanId?: number | null;
  assignedDietPlanId?: number | null;
  workoutPlanId?: number | null;
  assignedWorkoutPlanId?: number | null;
  notes?: string | null;
  takenAt?: string | null;
  createdAt: string;
  publicUrl: string;
  originalFileName: string;
  contentType: string;
  fileSizeBytes: number;
}

export interface TrainerFile {
  trainerFileId: number;
  trainerId: number;
  fileId: number;
  fileCategory: string;
  createdAt: string;
  publicUrl: string;
  originalFileName: string;
  contentType: string;
}

export interface UploadFileRequest {
  fileCategory: FileCategory;
  gymId?: string | null;
  memberId?: number | null;
  trainerId?: number | null;
  dietPlanId?: number | null;
  assignedDietPlanId?: number | null;
  workoutPlanId?: number | null;
  assignedWorkoutPlanId?: number | null;
  notes?: string | null;
  takenAt?: string | null;
}
