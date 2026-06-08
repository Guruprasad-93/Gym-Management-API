using Gym.Application.DTOs.DietPlans;

namespace Gym.Application.Interfaces;

public interface IDietPlanReportExporter
{
    byte[] ExportPdf(DietPlanDetailDto plan);
    byte[] ExportExcel(DietPlanDetailDto plan);
}
