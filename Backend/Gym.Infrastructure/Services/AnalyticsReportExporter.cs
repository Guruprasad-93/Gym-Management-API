using ClosedXML.Excel;
using Gym.Application.DTOs.Analytics;
using Gym.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gym.Infrastructure.Services;

public class AnalyticsReportExporter : IAnalyticsReportExporter
{
    static AnalyticsReportExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportDashboardPdf(AnalyticsDashboardDto dashboard, string gymName) =>
        BuildPdf($"{gymName} — Business Analytics", page =>
        {
            page.Content().Column(col =>
            {
                AddSection(col, "Overview KPIs");
                col.Item().Text($"Total Members: {dashboard.Overview.TotalMembers}");
                col.Item().Text($"Active Members: {dashboard.Overview.ActiveMembers}");
                col.Item().Text($"Revenue Today: {dashboard.Overview.RevenueToday:N2}");
                col.Item().Text($"Revenue This Month: {dashboard.Overview.RevenueThisMonth:N2}");
                col.Item().Text($"Expiring Memberships: {dashboard.Overview.ExpiringMemberships}");
                col.Item().Text($"Active Trainers: {dashboard.Overview.ActiveTrainers}");

                AddSection(col, "Revenue");
                col.Item().Text($"Today: {dashboard.Revenue.RevenueToday:N2} | Week: {dashboard.Revenue.RevenueThisWeek:N2} | Month: {dashboard.Revenue.RevenueThisMonth:N2} | Year: {dashboard.Revenue.RevenueThisYear:N2}");
                col.Item().Text($"Failed Payments: {dashboard.Revenue.FailedPaymentsCount}");

                AddSection(col, "Membership");
                col.Item().Text($"Active: {dashboard.Membership.ActiveMembers} | Expired: {dashboard.Membership.ExpiredMembers} | Expiring (7d): {dashboard.Membership.ExpiringIn7Days}");
                col.Item().Text($"New This Month: {dashboard.Membership.NewRegistrationsThisMonth}");

                AddSection(col, "Attendance");
                col.Item().Text($"Today: {dashboard.Attendance.TodayAttendanceCount} check-ins ({dashboard.Attendance.UniqueMembersToday} unique members)");

                AddSection(col, "Trainers");
                col.Item().Text($"Active Trainers: {dashboard.Trainers.ActiveTrainers} | Assigned Members: {dashboard.Trainers.AssignedMembers}");

                AddSection(col, "Workouts & Diet");
                col.Item().Text($"Workout completion: {dashboard.Workouts.CompletionPercentage:N1}% ({dashboard.Workouts.CompletedWorkoutPlans}/{dashboard.Workouts.ActiveWorkoutPlans + dashboard.Workouts.CompletedWorkoutPlans})");
                col.Item().Text($"Active diet plans: {dashboard.Diets.ActiveDietPlans} | Compliance: {dashboard.Diets.CompliancePercentage:N1}%");
            });
        });

    public byte[] ExportDashboardExcel(AnalyticsDashboardDto dashboard, string gymName)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Overview");
        ws.Cell(1, 1).Value = $"{gymName} — Business Analytics";
        ws.Cell(3, 1).Value = "Metric";
        ws.Cell(3, 2).Value = "Value";
        ws.Cell(4, 1).Value = "Total Members"; ws.Cell(4, 2).Value = dashboard.Overview.TotalMembers;
        ws.Cell(5, 1).Value = "Active Members"; ws.Cell(5, 2).Value = dashboard.Overview.ActiveMembers;
        ws.Cell(6, 1).Value = "Revenue Today"; ws.Cell(6, 2).Value = dashboard.Revenue.RevenueToday;
        ws.Cell(7, 1).Value = "Revenue This Month"; ws.Cell(7, 2).Value = dashboard.Revenue.RevenueThisMonth;
        ws.Cell(8, 1).Value = "Expiring Memberships"; ws.Cell(8, 2).Value = dashboard.Overview.ExpiringMemberships;
        ws.Cell(9, 1).Value = "Active Trainers"; ws.Cell(9, 2).Value = dashboard.Overview.ActiveTrainers;
        ws.Cell(10, 1).Value = "Failed Payments"; ws.Cell(10, 2).Value = dashboard.Revenue.FailedPaymentsCount;
        ws.Cell(11, 1).Value = "New Members This Month"; ws.Cell(11, 2).Value = dashboard.Membership.NewRegistrationsThisMonth;
        ws.Cell(12, 1).Value = "Today Attendance"; ws.Cell(12, 2).Value = dashboard.Attendance.TodayAttendanceCount;
        ws.Cell(13, 1).Value = "Workout Completion %"; ws.Cell(13, 2).Value = dashboard.Workouts.CompletionPercentage;
        ws.Cell(14, 1).Value = "Diet Compliance %"; ws.Cell(14, 2).Value = dashboard.Diets.CompliancePercentage;

        AddTrendSheet(wb, "Revenue Trend", dashboard.Revenue.RevenueTrend.Select(t => (t.MonthLabel, t.Value)));
        AddCountSheet(wb, "Plan Distribution", dashboard.Membership.PlanDistribution.Select(p => (p.Name, p.Count)));
        AddCountSheet(wb, "Payment Methods", dashboard.Revenue.RevenueByPaymentMethod.Select(p => (p.Name, p.Count)));

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportRevenuePdf(RevenueAnalyticsDto revenue, string gymName) =>
        BuildPdf($"{gymName} — Revenue Analytics", page =>
        {
            page.Content().Column(col =>
            {
                col.Item().Text($"Today: {revenue.RevenueToday:N2}");
                col.Item().Text($"This Week: {revenue.RevenueThisWeek:N2}");
                col.Item().Text($"This Month: {revenue.RevenueThisMonth:N2}");
                col.Item().Text($"This Year: {revenue.RevenueThisYear:N2}");
                col.Item().Text($"Failed Payments: {revenue.FailedPaymentsCount}");
                AddSection(col, "Monthly Trend");
                foreach (var point in revenue.RevenueTrend)
                    col.Item().Text($"{point.MonthLabel}: {point.Value:N2}").FontSize(9);
                AddSection(col, "By Plan");
                foreach (var item in revenue.RevenueByPlan)
                    col.Item().Text($"{item.Name}: {item.Value:N2} ({item.Count} payments)").FontSize(9);
            });
        });

    public byte[] ExportRevenueExcel(RevenueAnalyticsDto revenue, string gymName)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Summary");
        ws.Cell(1, 1).Value = $"{gymName} — Revenue Analytics";
        ws.Cell(3, 1).Value = "Today"; ws.Cell(3, 2).Value = revenue.RevenueToday;
        ws.Cell(4, 1).Value = "This Week"; ws.Cell(4, 2).Value = revenue.RevenueThisWeek;
        ws.Cell(5, 1).Value = "This Month"; ws.Cell(5, 2).Value = revenue.RevenueThisMonth;
        ws.Cell(6, 1).Value = "This Year"; ws.Cell(6, 2).Value = revenue.RevenueThisYear;
        ws.Cell(7, 1).Value = "Failed Payments"; ws.Cell(7, 2).Value = revenue.FailedPaymentsCount;

        AddTrendSheet(wb, "Monthly Trend", revenue.RevenueTrend.Select(t => (t.MonthLabel, t.Value)));
        AddTrendSheet(wb, "By Plan", revenue.RevenueByPlan.Select(p => (p.Name, p.Value)));
        AddTrendSheet(wb, "By Payment Method", revenue.RevenueByPaymentMethod.Select(p => (p.Name, p.Value)));

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] BuildPdf(string title, Action<PageDescriptor> configurePage)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Header().Text(title).Bold().FontSize(14);
                configurePage(page);
                page.Footer().AlignCenter().Text(x => { x.Span("Page "); x.CurrentPageNumber(); });
            });
        });
        return doc.GeneratePdf();
    }

    private static void AddSection(ColumnDescriptor col, string title)
    {
        col.Item().PaddingTop(8).Text(title).Bold().FontSize(11);
    }

    private static void AddTrendSheet(XLWorkbook wb, string name, IEnumerable<(string Label, decimal Value)> rows)
    {
        var ws = wb.Worksheets.Add(name.Length > 31 ? name[..31] : name);
        ws.Cell(1, 1).Value = "Label";
        ws.Cell(1, 2).Value = "Value";
        var r = 2;
        foreach (var (label, value) in rows)
        {
            ws.Cell(r, 1).Value = label;
            ws.Cell(r, 2).Value = value;
            r++;
        }
    }

    private static void AddCountSheet(XLWorkbook wb, string name, IEnumerable<(string Label, int Count)> rows)
    {
        var ws = wb.Worksheets.Add(name.Length > 31 ? name[..31] : name);
        ws.Cell(1, 1).Value = "Label";
        ws.Cell(1, 2).Value = "Count";
        var r = 2;
        foreach (var (label, count) in rows)
        {
            ws.Cell(r, 1).Value = label;
            ws.Cell(r, 2).Value = count;
            r++;
        }
    }
}
