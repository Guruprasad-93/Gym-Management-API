using ClosedXML.Excel;
using Gym.Application.DTOs.Audit;
using Gym.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gym.Infrastructure.Services;

public class AuditReportExporter : IAuditReportExporter
{
    static AuditReportExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportPdf(IReadOnlyList<AuditLogDto> logs, string title)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Header().Text(title).Bold().FontSize(16);
                page.Content().PaddingTop(10).Column(col =>
                {
                    foreach (var log in logs.Take(500))
                    {
                        col.Item().Text($"{log.CreatedDate:yyyy-MM-dd HH:mm} | {log.UserName ?? "System"} | {log.EntityName}#{log.EntityId} | {log.ActionType}").FontSize(9);
                        if (!string.IsNullOrEmpty(log.IpAddress))
                            col.Item().Text($"IP: {log.IpAddress}").FontSize(8).FontColor(Colors.Grey.Medium);
                    }
                });
                page.Footer().AlignCenter().Text(x => { x.Span("Page "); x.CurrentPageNumber(); });
            });
        });
        return doc.GeneratePdf();
    }

    public byte[] ExportExcel(IReadOnlyList<AuditLogDto> logs, string title)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Audit Logs");
        ws.Cell(1, 1).Value = title;
        var headers = new[] { "Date", "User", "Email", "Gym", "Entity", "Entity Id", "Action", "IP", "Old JSON", "New JSON" };
        for (var c = 0; c < headers.Length; c++)
            ws.Cell(2, c + 1).Value = headers[c];
        var row = 3;
        foreach (var log in logs)
        {
            ws.Cell(row, 1).Value = log.CreatedDate;
            ws.Cell(row, 2).Value = log.UserName ?? "";
            ws.Cell(row, 3).Value = log.UserEmail ?? "";
            ws.Cell(row, 4).Value = log.GymName ?? "";
            ws.Cell(row, 5).Value = log.EntityName;
            ws.Cell(row, 6).Value = log.EntityId;
            ws.Cell(row, 7).Value = log.ActionType;
            ws.Cell(row, 8).Value = log.IpAddress ?? "";
            ws.Cell(row, 9).Value = log.OldValueJson ?? "";
            ws.Cell(row, 10).Value = log.NewValueJson ?? "";
            row++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
