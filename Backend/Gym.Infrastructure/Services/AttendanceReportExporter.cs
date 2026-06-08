using ClosedXML.Excel;
using Gym.Application.DTOs.Attendance;
using Gym.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gym.Infrastructure.Services;

public class AttendanceReportExporter : IAttendanceReportExporter
{
    static AttendanceReportExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportDailyReportPdf(DailyAttendanceReportDto report) =>
        BuildPdf($"Daily Attendance — {report.ReportDate:yyyy-MM-dd}", container =>
        {
            container.Item().Text("Status summary").Bold();
            foreach (var s in report.StatusCounts)
                container.Item().Text($"{s.StatusName}: {s.RecordCount}");
            container.Item().PaddingTop(10).Text("Details").Bold();
            foreach (var d in report.Details)
                container.Item().Text($"{d.MemberName} — {d.StatusName} — In: {d.CheckInAt:HH:mm} Out: {d.CheckOutAt:HH:mm}");
        });

    public byte[] ExportMonthlyReportPdf(MonthlyAttendanceReportDto report) =>
        BuildPdf($"Monthly Attendance — {report.Year}-{report.Month:D2}", container =>
        {
            container.Item().Text("Member").Bold();
            foreach (var m in report.Members)
                container.Item().Text($"{m.MemberName} — Present: {m.PresentDays}, Absent: {m.AbsentDays}, Late: {m.LateDays}");
        });

    public byte[] ExportMemberHistoryExcel(IReadOnlyList<MemberAttendanceDto> records, string memberName)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Member History");
        ws.Cell(1, 1).Value = $"Attendance History — {memberName}";
        ws.Cell(2, 1).Value = "Date";
        ws.Cell(2, 2).Value = "Status";
        ws.Cell(2, 3).Value = "Check In";
        ws.Cell(2, 4).Value = "Check Out";
        ws.Cell(2, 5).Value = "Notes";
        var row = 3;
        foreach (var r in records)
        {
            ws.Cell(row, 1).Value = r.AttendanceDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 2).Value = r.StatusName;
            ws.Cell(row, 3).Value = r.CheckInAt?.ToString("yyyy-MM-dd HH:mm") ?? "";
            ws.Cell(row, 4).Value = r.CheckOutAt?.ToString("yyyy-MM-dd HH:mm") ?? "";
            ws.Cell(row, 5).Value = r.Notes ?? "";
            row++;
        }
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportDailyReportExcel(DailyAttendanceReportDto report)
    {
        using var wb = new XLWorkbook();
        var summary = wb.Worksheets.Add("Summary");
        summary.Cell(1, 1).Value = $"Daily Report {report.ReportDate:yyyy-MM-dd}";
        summary.Cell(2, 1).Value = "Status";
        summary.Cell(2, 2).Value = "Count";
        var r = 3;
        foreach (var s in report.StatusCounts)
        {
            summary.Cell(r, 1).Value = s.StatusName;
            summary.Cell(r, 2).Value = s.RecordCount;
            r++;
        }
        var details = wb.Worksheets.Add("Details");
        details.Cell(1, 1).Value = "Member";
        details.Cell(1, 2).Value = "Status";
        details.Cell(1, 3).Value = "Check In";
        details.Cell(1, 4).Value = "Check Out";
        r = 2;
        foreach (var d in report.Details)
        {
            details.Cell(r, 1).Value = d.MemberName;
            details.Cell(r, 2).Value = d.StatusName;
            details.Cell(r, 3).Value = d.CheckInAt?.ToString("HH:mm") ?? "";
            details.Cell(r, 4).Value = d.CheckOutAt?.ToString("HH:mm") ?? "";
            r++;
        }
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportMonthlyReportExcel(MonthlyAttendanceReportDto report)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add($"Monthly {report.Year}-{report.Month}");
        ws.Cell(1, 1).Value = "Member";
        ws.Cell(1, 2).Value = "Present";
        ws.Cell(1, 3).Value = "Absent";
        ws.Cell(1, 4).Value = "Late";
        ws.Cell(1, 5).Value = "Excused";
        ws.Cell(1, 6).Value = "Total";
        var row = 2;
        foreach (var m in report.Members)
        {
            ws.Cell(row, 1).Value = m.MemberName;
            ws.Cell(row, 2).Value = m.PresentDays;
            ws.Cell(row, 3).Value = m.AbsentDays;
            ws.Cell(row, 4).Value = m.LateDays;
            ws.Cell(row, 5).Value = m.ExcusedDays;
            ws.Cell(row, 6).Value = m.TotalRecords;
            row++;
        }
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static byte[] BuildPdf(string title, Action<ColumnDescriptor> content)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Header().Text(title).Bold().FontSize(18);
                page.Content().PaddingTop(15).Column(content);
            });
        });
        return doc.GeneratePdf();
    }
}
