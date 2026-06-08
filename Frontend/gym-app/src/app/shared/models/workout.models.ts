export interface ExerciseCategory {
  exerciseCategoryId: number;
  gymId: string;
  categoryName: string;
  description?: string;
  isActive: boolean;
}

export interface Exercise {
  exerciseId: number;
  gymId: string;
  exerciseCategoryId?: number;
  categoryName?: string;
  exerciseName: string;
  muscleGroup?: string;
  difficulty?: string;
  instructions?: string;
  isActive: boolean;
}

export interface WorkoutPlanListItem {
  workoutPlanId: number;
  planName: string;
  goal?: string;
  durationWeeks?: number;
  isActive: boolean;
  exerciseCount: number;
  activeAssignmentCount: number;
}

export interface WorkoutPlanExercise {
  workoutPlanExerciseId: number;
  dayNumber: number;
  exerciseId: number;
  exerciseName: string;
  muscleGroup?: string;
  sets?: number;
  reps?: string;
  weight?: string;
  restSeconds?: number;
  notes?: string;
  sortOrder: number;
  isCompleted?: boolean;
  completionPercentage?: number;
  trainerNotes?: string;
  memberNotes?: string;
}

export interface WorkoutPlanDetail {
  workoutPlanId: number;
  planName: string;
  description?: string;
  goal?: string;
  durationWeeks?: number;
  isActive: boolean;
  exercises: WorkoutPlanExercise[];
}

export interface WorkoutPlanExerciseInput {
  dayNumber: number;
  exerciseId: number;
  sets?: number;
  reps?: string;
  weight?: string;
  restSeconds?: number;
  notes?: string;
  sortOrder: number;
}

export interface MemberWorkoutView {
  assignedWorkoutPlanId?: number;
  memberId: number;
  memberName?: string;
  workoutPlanId?: number;
  planName?: string;
  planDescription?: string;
  goal?: string;
  durationWeeks?: number;
  startDate?: string;
  endDate?: string;
  overallCompletionPercentage: number;
  exercises: WorkoutPlanExercise[];
}

export interface AssignWorkoutPlanRequest {
  memberId: number;
  workoutPlanId: number;
  startDate: string;
  endDate?: string;
  notes?: string;
}

export interface UpdateWorkoutProgressRequest {
  memberId: number;
  assignedWorkoutPlanId: number;
  workoutPlanExerciseId: number;
  isCompleted?: boolean;
  completionPercentage?: number;
  trainerNotes?: string;
  memberNotes?: string;
}

export const DIFFICULTIES = ['Beginner', 'Intermediate', 'Advanced'] as const;
