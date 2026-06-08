using ClosedXML.Excel;
using Gym.Application.DTOs.Booking;
using Gym.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gym.Infrastructure.Services;

public class BookingReportExporter : IBookingReportExporter
{
    static BookingReportExporter() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] ExportBookingsPdf(IReadOnlyList<SlotBookingDto> bookings, string title) =>
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
                        h.Cell().Text("Member");
                        h.Cell().Text("Class");
                        h.Cell().Text("Date");
                        h.Cell().Text("Status");
                    });
                    foreach (var b in bookings)
                    {
                        table.Cell().Text(b.MemberName);
                        table.Cell().Text(b.ClassName);
                        table.Cell().Text(b.BookingDate.ToString("yyyy-MM-dd"));
                        table.Cell().Text(b.Status);
                    }
                });
            });
        }).GeneratePdf();

    public byte[] ExportBookingsExcel(IReadOnlyList<SlotBookingDto> bookings, string title)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Bookings");
        ws.Cell(1, 1).Value = title;
        ws.Cell(2, 1).Value = "Member";
        ws.Cell(2, 2).Value = "Class";
        ws.Cell(2, 3).Value = "Date";
        ws.Cell(2, 4).Value = "Status";
        ws.Cell(2, 5).Value = "Branch";
        var row = 3;
        foreach (var b in bookings)
        {
            ws.Cell(row, 1).Value = b.MemberName;
            ws.Cell(row, 2).Value = b.ClassName;
            ws.Cell(row, 3).Value = b.BookingDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 4).Value = b.Status;
            ws.Cell(row, 5).Value = b.BranchName;
            row++;
        }
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportOccupancyPdf(BookingAnalyticsDto analytics, string title) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Content().Column(col =>
                {
                    col.Item().Text(title).FontSize(18).Bold();
                    col.Item().Text($"Total bookings: {analytics.TotalBookings}");
                    col.Item().Text($"Occupancy: {analytics.OccupancyPercent:0.##}%");
                    col.Item().Text($"No-show: {analytics.NoShowPercent:0.##}%");
                    col.Item().Text($"Cancellation: {analytics.CancellationPercent:0.##}%");
                });
            });
        }).GeneratePdf();

    public byte[] ExportOccupancyExcel(BookingAnalyticsDto analytics, string title)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Occupancy");
        ws.Cell(1, 1).Value = title;
        ws.Cell(2, 1).Value = "Total Bookings";
        ws.Cell(2, 2).Value = analytics.TotalBookings;
        ws.Cell(3, 1).Value = "Occupancy %";
        ws.Cell(3, 2).Value = analytics.OccupancyPercent;
        ws.Cell(4, 1).Value = "No-show %";
        ws.Cell(4, 2).Value = analytics.NoShowPercent;
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }
}
