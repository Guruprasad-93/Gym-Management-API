using Gym.Application.DTOs.WorkoutPlans;

namespace Gym.Application.Interfaces;

public interface IWorkoutPlanReportExporter
{
    byte[] ExportPdf(WorkoutPlanDetailDto plan);
    byte[] ExportExcel(WorkoutPlanDetailDto plan);
}
