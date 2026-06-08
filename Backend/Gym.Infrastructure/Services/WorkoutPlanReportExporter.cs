using ClosedXML.Excel;
using Gym.Application.DTOs.WorkoutPlans;
using Gym.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gym.Infrastructure.Services;

public class WorkoutPlanReportExporter : IWorkoutPlanReportExporter
{
    static WorkoutPlanReportExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportPdf(WorkoutPlanDetailDto plan)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Column(col =>
                {
                    col.Item().Text(plan.PlanName).Bold().FontSize(18);
                    if (!string.IsNullOrWhiteSpace(plan.Goal))
                        col.Item().Text($"Goal: {plan.Goal}").FontSize(10);
                    if (plan.DurationWeeks.HasValue)
                        col.Item().Text($"Duration: {plan.DurationWeeks} weeks").FontSize(10);
                });
                page.Content().PaddingTop(10).Column(col =>
                {
                    foreach (var day in plan.Exercises.GroupBy(e => e.DayNumber).OrderBy(g => g.Key))
                    {
                        col.Item().PaddingTop(8).Text($"Day {day.Key}").Bold().FontSize(12);
                        foreach (var ex in day.OrderBy(e => e.SortOrder))
                        {
                            col.Item().Text($"{ex.ExerciseName} — {ex.Sets}x{ex.Reps} @ {ex.Weight ?? "—"} (rest {ex.RestSeconds}s)").FontSize(9);
                        }
                    }
                });
            });
        });
        return doc.GeneratePdf();
    }

    public byte[] ExportExcel(WorkoutPlanDetailDto plan)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Workout Plan");
        ws.Cell(1, 1).Value = plan.PlanName;
        ws.Cell(2, 1).Value = "Goal";
        ws.Cell(2, 2).Value = plan.Goal ?? "";
        ws.Cell(3, 1).Value = "Weeks";
        ws.Cell(3, 2).Value = plan.DurationWeeks ?? 0;
        ws.Cell(5, 1).Value = "Day";
        ws.Cell(5, 2).Value = "Exercise";
        ws.Cell(5, 3).Value = "Sets";
        ws.Cell(5, 4).Value = "Reps";
        ws.Cell(5, 5).Value = "Weight";
        ws.Cell(5, 6).Value = "Rest (s)";
        ws.Cell(5, 7).Value = "Notes";
        var row = 6;
        foreach (var ex in plan.Exercises.OrderBy(e => e.DayNumber).ThenBy(e => e.SortOrder))
        {
            ws.Cell(row, 1).Value = ex.DayNumber;
            ws.Cell(row, 2).Value = ex.ExerciseName;
            ws.Cell(row, 3).Value = ex.Sets ?? 0;
            ws.Cell(row, 4).Value = ex.Reps ?? "";
            ws.Cell(row, 5).Value = ex.Weight ?? "";
            ws.Cell(row, 6).Value = ex.RestSeconds ?? 0;
            ws.Cell(row, 7).Value = ex.Notes ?? "";
            row++;
        }
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
