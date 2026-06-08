using Gym.Application.DTOs.Attendance;
using Gym.Application.DTOs.MemberSelfService;
using Gym.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gym.Infrastructure.Services;

public class MemberSelfServiceReportExporter : IMemberSelfServiceReportExporter
{
    static MemberSelfServiceReportExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportProgressPdf(string memberName, IReadOnlyList<MemberProgressEntryDto> entries) =>
        BuildPdf($"Progress Report — {memberName}", col =>
        {
            col.Item().Text("Body Measurements").Bold();
            foreach (var e in entries)
            {
                col.Item().Text($"{e.ProgressDate:yyyy-MM-dd} — Weight: {e.Weight} kg, BMI: {e.Bmi}, Waist: {e.Waist} cm");
                if (!string.IsNullOrWhiteSpace(e.Notes))
                    col.Item().Text($"  Notes: {e.Notes}").FontSize(9);
            }
        });

    public byte[] ExportAttendancePdf(string memberName, IReadOnlyList<MemberAttendanceDto> records) =>
        BuildPdf($"Attendance Report — {memberName}", col =>
        {
            foreach (var r in records)
                col.Item().Text($"{r.AttendanceDate:yyyy-MM-dd} — {r.StatusName} — In: {r.CheckInAt:HH:mm} Out: {r.CheckOutAt:HH:mm}");
        });

    public byte[] ExportGoalSummaryPdf(string memberName, IReadOnlyList<MemberGoalDto> goals) =>
        BuildPdf($"Goal Summary — {memberName}", col =>
        {
            foreach (var g in goals)
                col.Item().Text($"{g.GoalType} — Target: {g.TargetValue}, Current: {g.CurrentValue}, Status: {g.Status}, Target Date: {g.TargetDate:yyyy-MM-dd}");
        });

    private static byte[] BuildPdf(string title, Action<ColumnDescriptor> content)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text(title).Bold().FontSize(16);
                page.Content().Column(content);
                page.Footer().AlignCenter().Text($"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm UTC}").FontSize(8);
            });
        });
        return doc.GeneratePdf();
    }
}
