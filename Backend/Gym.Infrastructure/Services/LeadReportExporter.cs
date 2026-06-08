using ClosedXML.Excel;
using Gym.Application.DTOs.Leads;
using Gym.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gym.Infrastructure.Services;

public class LeadReportExporter : ILeadReportExporter
{
    static LeadReportExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportLeadSummaryPdf(IReadOnlyList<LeadDto> leads, string title) =>
        BuildPdf(title, page =>
        {
            page.Content().Column(col =>
            {
                col.Item().Text($"Total leads: {leads.Count}").FontSize(10);
                foreach (var lead in leads.Take(500))
                {
                    col.Item().PaddingTop(4).Text(
                        $"{lead.FullName} | {lead.MobileNumber} | {lead.LeadSource} | {lead.Status} | {lead.CreatedDate:yyyy-MM-dd}")
                        .FontSize(9);
                }
            });
        });

    public byte[] ExportLeadSummaryExcel(IReadOnlyList<LeadDto> leads, string title)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Leads");
        ws.Cell(1, 1).Value = title;
        var headers = new[] { "Name", "Mobile", "Email", "Source", "Status", "Trainer", "Plan", "Created" };
        for (var c = 0; c < headers.Length; c++)
            ws.Cell(3, c + 1).Value = headers[c];
        var row = 4;
        foreach (var lead in leads)
        {
            ws.Cell(row, 1).Value = lead.FullName;
            ws.Cell(row, 2).Value = lead.MobileNumber;
            ws.Cell(row, 3).Value = lead.Email ?? "";
            ws.Cell(row, 4).Value = lead.LeadSource;
            ws.Cell(row, 5).Value = lead.Status;
            ws.Cell(row, 6).Value = lead.AssignedTrainerName ?? "";
            ws.Cell(row, 7).Value = lead.InterestedPlanName ?? "";
            ws.Cell(row, 8).Value = lead.CreatedDate;
            row++;
        }
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportConversionReportPdf(LeadAnalyticsDto analytics, string title) =>
        BuildPdf(title, page =>
        {
            page.Content().Column(col =>
            {
                var d = analytics.Dashboard;
                col.Item().Text($"Total Leads: {d.TotalLeads} | New Today: {d.NewLeadsToday} | Conversion: {d.ConversionRate:N1}%").FontSize(10);
                col.Item().Text($"Trial Conversion: {d.TrialConversionRate:N1}% | Lost: {d.LostLeads} | Pending Follow-ups: {d.PendingFollowUps}").FontSize(10);
                AddSection(col, "By Source");
                foreach (var item in analytics.LeadsBySource)
                    col.Item().Text($"{item.Name}: {item.Count}").FontSize(9);
                AddSection(col, "Monthly Conversions");
                foreach (var point in analytics.MonthlyConversions)
                    col.Item().Text($"{point.MonthLabel}: {point.Conversions} conversions / {point.NewLeads} new leads").FontSize(9);
            });
        });

    public byte[] ExportConversionReportExcel(LeadAnalyticsDto analytics, string title)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Summary");
        ws.Cell(1, 1).Value = title;
        var d = analytics.Dashboard;
        ws.Cell(3, 1).Value = "Total Leads"; ws.Cell(3, 2).Value = d.TotalLeads;
        ws.Cell(4, 1).Value = "New Today"; ws.Cell(4, 2).Value = d.NewLeadsToday;
        ws.Cell(5, 1).Value = "Conversion Rate %"; ws.Cell(5, 2).Value = d.ConversionRate;
        ws.Cell(6, 1).Value = "Trial Conversion %"; ws.Cell(6, 2).Value = d.TrialConversionRate;
        ws.Cell(7, 1).Value = "Lost Leads"; ws.Cell(7, 2).Value = d.LostLeads;

        var src = wb.Worksheets.Add("By Source");
        src.Cell(1, 1).Value = "Source";
        src.Cell(1, 2).Value = "Count";
        var r = 2;
        foreach (var item in analytics.LeadsBySource)
        {
            src.Cell(r, 1).Value = item.Name;
            src.Cell(r, 2).Value = item.Count;
            r++;
        }

        var monthly = wb.Worksheets.Add("Monthly");
        monthly.Cell(1, 1).Value = "Month";
        monthly.Cell(1, 2).Value = "New Leads";
        monthly.Cell(1, 3).Value = "Conversions";
        r = 2;
        foreach (var point in analytics.MonthlyConversions)
        {
            monthly.Cell(r, 1).Value = point.MonthLabel;
            monthly.Cell(r, 2).Value = point.NewLeads;
            monthly.Cell(r, 3).Value = point.Conversions;
            r++;
        }

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportFollowUpReportExcel(IReadOnlyList<LeadFollowUpDto> followUps, string title)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Follow-ups");
        ws.Cell(1, 1).Value = title;
        var headers = new[] { "Lead", "Mobile", "Date", "Type", "Status", "Remarks", "Next Follow-up" };
        for (var c = 0; c < headers.Length; c++)
            ws.Cell(3, c + 1).Value = headers[c];
        var row = 4;
        foreach (var f in followUps)
        {
            ws.Cell(row, 1).Value = f.LeadName ?? "";
            ws.Cell(row, 2).Value = f.MobileNumber ?? "";
            ws.Cell(row, 3).Value = f.FollowUpDate;
            ws.Cell(row, 4).Value = f.FollowUpType;
            ws.Cell(row, 5).Value = f.Status;
            ws.Cell(row, 6).Value = f.Remarks ?? "";
            ws.Cell(row, 7).Value = f.NextFollowUpDate?.ToString("yyyy-MM-dd HH:mm") ?? "";
            row++;
        }
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] BuildPdf(string title, Action<PageDescriptor> configure)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Header().Text(title).Bold().FontSize(16);
                configure(page);
                page.Footer().AlignCenter().Text(x => { x.Span("Page "); x.CurrentPageNumber(); });
            });
        });
        return doc.GeneratePdf();
    }

    private static void AddSection(ColumnDescriptor col, string title)
    {
        col.Item().PaddingTop(10).Text(title).Bold().FontSize(11);
    }
}
