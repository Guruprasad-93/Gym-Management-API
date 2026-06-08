export interface DietCategory {
  dietCategoryId: number;
  gymId: string;
  categoryName: string;
  description?: string;
  isActive: boolean;
}

export interface DietPlanItem {
  dietPlanItemId?: number;
  dietPlanId?: number;
  mealTime: string;
  foodName: string;
  quantity?: string;
  calories?: number;
  notes?: string;
  sortOrder: number;
}

export interface DietPlanListItem {
  dietPlanId: number;
  gymId: string;
  dietCategoryId?: number;
  categoryName?: string;
  planName: string;
  description?: string;
  targetCalories?: number;
  isActive: boolean;
  itemCount: number;
  activeAssignmentCount: number;
}

export interface DietPlanDetail {
  dietPlanId: number;
  gymId: string;
  dietCategoryId?: number;
  categoryName?: string;
  planName: string;
  description?: string;
  targetCalories?: number;
  isActive: boolean;
  items: DietPlanItem[];
}

export interface CreateDietPlanRequest {
  gymId?: string;
  planName: string;
  description?: string;
  dietCategoryId?: number;
  targetCalories?: number;
  isActive: boolean;
  items: DietPlanItem[];
}

export interface AssignDietPlanRequest {
  memberId: number;
  dietPlanId: number;
  startDate: string;
  endDate?: string;
  notes?: string;
  deactivatePrevious?: boolean;
}

export interface MemberDietPlanView {
  assignedDietPlanId?: number;
  memberId: number;
  memberName?: string;
  dietPlanId?: number;
  planName?: string;
  planDescription?: string;
  targetCalories?: number;
  categoryName?: string;
  startDate?: string;
  endDate?: string;
  assignmentNotes?: string;
  isActive: boolean;
  items: DietPlanItem[];
}

export const MEAL_TIMES = ['Breakfast', 'Morning Snack', 'Lunch', 'Afternoon Snack', 'Dinner', 'Evening Snack'] as const;
