using ClosedXML.Excel;
using Gym.Application.DTOs.Website;
using Gym.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gym.Infrastructure.Services;

public class WebsiteReportExporter : IWebsiteReportExporter
{
    static WebsiteReportExporter() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] ExportLeadsPdf(IReadOnlyList<WebsiteLeadCaptureDto> leads, string title) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text(title).FontSize(18).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                    });
                    table.Header(h =>
                    {
                        h.Cell().Text("Name");
                        h.Cell().Text("Mobile");
                        h.Cell().Text("Source");
                        h.Cell().Text("Status");
                    });
                    foreach (var lead in leads)
                    {
                        table.Cell().Text(lead.Name);
                        table.Cell().Text(lead.MobileNumber);
                        table.Cell().Text(lead.Source);
                        table.Cell().Text(lead.Status);
                    }
                });
            });
        }).GeneratePdf();

    public byte[] ExportLeadsExcel(IReadOnlyList<WebsiteLeadCaptureDto> leads, string title)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Website Leads");
        ws.Cell(1, 1).Value = title;
        ws.Cell(2, 1).Value = "Name";
        ws.Cell(2, 2).Value = "Mobile";
        ws.Cell(2, 3).Value = "Email";
        ws.Cell(2, 4).Value = "Source";
        ws.Cell(2, 5).Value = "Status";
        ws.Cell(2, 6).Value = "Interested Plan";
        ws.Cell(2, 7).Value = "Created";
        var row = 3;
        foreach (var lead in leads)
        {
            ws.Cell(row, 1).Value = lead.Name;
            ws.Cell(row, 2).Value = lead.MobileNumber;
            ws.Cell(row, 3).Value = lead.Email;
            ws.Cell(row, 4).Value = lead.Source;
            ws.Cell(row, 5).Value = lead.Status;
            ws.Cell(row, 6).Value = lead.InterestedPlan;
            ws.Cell(row, 7).Value = lead.CreatedDate.ToString("yyyy-MM-dd");
            row++;
        }
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }
}
