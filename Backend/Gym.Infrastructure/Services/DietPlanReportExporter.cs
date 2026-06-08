using ClosedXML.Excel;
using Gym.Application.DTOs.DietPlans;
using Gym.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gym.Infrastructure.Services;

public class DietPlanReportExporter : IDietPlanReportExporter
{
    static DietPlanReportExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportPdf(DietPlanDetailDto plan)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Header().Column(col =>
                {
                    col.Item().Text(plan.PlanName).Bold().FontSize(18);
                    if (!string.IsNullOrWhiteSpace(plan.CategoryName))
                        col.Item().Text($"Category: {plan.CategoryName}").FontSize(10);
                    if (plan.TargetCalories.HasValue)
                        col.Item().Text($"Target calories: {plan.TargetCalories}").FontSize(10);
                    if (!string.IsNullOrWhiteSpace(plan.Description))
                        col.Item().Text(plan.Description).FontSize(9);
                });
                page.Content().PaddingTop(12).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2);
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                    });
                    table.Header(h =>
                    {
                        h.Cell().Text("Meal").Bold();
                        h.Cell().Text("Food").Bold();
                        h.Cell().Text("Quantity").Bold();
                        h.Cell().Text("Cal").Bold();
                    });
                    foreach (var item in plan.Items)
                    {
                        table.Cell().Text(item.MealTime);
                        table.Cell().Text(item.FoodName);
                        table.Cell().Text(item.Quantity ?? "—");
                        table.Cell().Text(item.Calories?.ToString("0") ?? "—");
                    }
                });
            });
        });
        return doc.GeneratePdf();
    }

    public byte[] ExportExcel(DietPlanDetailDto plan)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Diet Plan");
        ws.Cell(1, 1).Value = plan.PlanName;
        ws.Cell(2, 1).Value = "Category";
        ws.Cell(2, 2).Value = plan.CategoryName ?? "";
        ws.Cell(3, 1).Value = "Target Calories";
        ws.Cell(3, 2).Value = plan.TargetCalories ?? 0;
        ws.Cell(5, 1).Value = "Meal Time";
        ws.Cell(5, 2).Value = "Food";
        ws.Cell(5, 3).Value = "Quantity";
        ws.Cell(5, 4).Value = "Calories";
        ws.Cell(5, 5).Value = "Notes";
        var row = 6;
        foreach (var item in plan.Items)
        {
            ws.Cell(row, 1).Value = item.MealTime;
            ws.Cell(row, 2).Value = item.FoodName;
            ws.Cell(row, 3).Value = item.Quantity ?? "";
            ws.Cell(row, 4).Value = item.Calories ?? 0;
            ws.Cell(row, 5).Value = item.Notes ?? "";
            row++;
        }
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
