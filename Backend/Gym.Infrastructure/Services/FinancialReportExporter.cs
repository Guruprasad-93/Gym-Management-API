using ClosedXML.Excel;
using Gym.Application.DTOs.Financial;
using Gym.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gym.Infrastructure.Services;

public class FinancialReportExporter : IFinancialReportExporter
{
    static FinancialReportExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportExpenseReportPdf(IReadOnlyList<ExpenseDto> expenses, string title) =>
        BuildPdf(title, col =>
        {
            col.Item().Text($"Total: {expenses.Sum(e => e.Amount):N2} ({expenses.Count} records)").FontSize(10);
            foreach (var e in expenses.Take(500))
                col.Item().PaddingTop(3).Text($"{e.ExpenseDate:yyyy-MM-dd} | {e.CategoryName} | {e.Amount:N2} | {e.VendorName ?? "-"}").FontSize(9);
        });

    public byte[] ExportExpenseLedgerExcel(IReadOnlyList<ExpenseDto> expenses, string title)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Expenses");
        ws.Cell(1, 1).Value = title;
        var headers = new[] { "Date", "Category", "Amount", "Vendor", "Method", "Description" };
        for (var c = 0; c < headers.Length; c++) ws.Cell(3, c + 1).Value = headers[c];
        var row = 4;
        foreach (var e in expenses)
        {
            ws.Cell(row, 1).Value = e.ExpenseDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 2).Value = e.CategoryName;
            ws.Cell(row, 3).Value = e.Amount;
            ws.Cell(row, 4).Value = e.VendorName ?? "";
            ws.Cell(row, 5).Value = e.PaymentMethod;
            ws.Cell(row, 6).Value = e.Description ?? "";
            row++;
        }
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportPayrollReportPdf(IReadOnlyList<PayrollDto> payrolls, string title) =>
        BuildPdf(title, col =>
        {
            col.Item().Text($"Total net: {payrolls.Sum(p => p.NetSalary):N2}").FontSize(10);
            foreach (var p in payrolls.Take(500))
                col.Item().PaddingTop(3).Text($"{p.EmployeeName} | {p.SalaryMonth:MMM yyyy} | {p.NetSalary:N2} | {p.Status}").FontSize(9);
        });

    public byte[] ExportPayrollLedgerExcel(IReadOnlyList<PayrollDto> payrolls, string title)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Payroll");
        ws.Cell(1, 1).Value = title;
        var headers = new[] { "Employee", "Type", "Month", "Base", "Incentive", "Commission", "Deduction", "Net", "Status", "Paid" };
        for (var c = 0; c < headers.Length; c++) ws.Cell(3, c + 1).Value = headers[c];
        var row = 4;
        foreach (var p in payrolls)
        {
            ws.Cell(row, 1).Value = p.EmployeeName;
            ws.Cell(row, 2).Value = p.EmployeeType;
            ws.Cell(row, 3).Value = p.SalaryMonth.ToString("yyyy-MM");
            ws.Cell(row, 4).Value = p.BaseSalary;
            ws.Cell(row, 5).Value = p.IncentiveAmount;
            ws.Cell(row, 6).Value = p.CommissionAmount;
            ws.Cell(row, 7).Value = p.DeductionAmount;
            ws.Cell(row, 8).Value = p.NetSalary;
            ws.Cell(row, 9).Value = p.Status;
            ws.Cell(row, 10).Value = p.PaidDate?.ToString("yyyy-MM-dd") ?? "";
            row++;
        }
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportProfitLossPdf(FinancialDashboardDto dashboard, string title) =>
        BuildPdf(title, col =>
        {
            col.Item().Text($"Revenue: {dashboard.RevenueThisMonth:N2} | Expenses: {dashboard.ExpensesThisMonth:N2} | Profit: {dashboard.ProfitThisMonth:N2}").FontSize(10);
            col.Item().Text($"Pending Salaries: {dashboard.PendingSalaries:N2} | Commissions: {dashboard.TotalTrainerCommissions:N2}").FontSize(10);
            AddSection(col, "P&L Summary");
            col.Item().Text($"Revenue: {dashboard.Summary.Revenue:N2} | Expenses: {dashboard.Summary.Expenses:N2} | Payroll: {dashboard.Summary.PayrollCost:N2} | Profit: {dashboard.Summary.Profit:N2}").FontSize(9);
            AddSection(col, "Monthly Trend");
            foreach (var m in dashboard.MonthlyProfitTrend)
                col.Item().Text($"{m.MonthLabel}: Rev {m.Revenue:N0} - Exp {m.Expenses:N0} - Pay {m.PayrollCost:N0} = {m.Profit:N0}").FontSize(9);
        });

    public byte[] ExportProfitLossExcel(FinancialDashboardDto dashboard, string title)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Summary");
        ws.Cell(1, 1).Value = title;
        ws.Cell(3, 1).Value = "Revenue This Month"; ws.Cell(3, 2).Value = dashboard.RevenueThisMonth;
        ws.Cell(4, 1).Value = "Expenses This Month"; ws.Cell(4, 2).Value = dashboard.ExpensesThisMonth;
        ws.Cell(5, 1).Value = "Profit This Month"; ws.Cell(5, 2).Value = dashboard.ProfitThisMonth;
        ws.Cell(6, 1).Value = "Pending Salaries"; ws.Cell(6, 2).Value = dashboard.PendingSalaries;
        ws.Cell(7, 1).Value = "Trainer Commissions"; ws.Cell(7, 2).Value = dashboard.TotalTrainerCommissions;

        var trend = wb.Worksheets.Add("Monthly Trend");
        trend.Cell(1, 1).Value = "Month"; trend.Cell(1, 2).Value = "Revenue"; trend.Cell(1, 3).Value = "Expenses"; trend.Cell(1, 4).Value = "Payroll"; trend.Cell(1, 5).Value = "Profit";
        var r = 2;
        foreach (var m in dashboard.MonthlyProfitTrend)
        {
            trend.Cell(r, 1).Value = m.MonthLabel;
            trend.Cell(r, 2).Value = m.Revenue;
            trend.Cell(r, 3).Value = m.Expenses;
            trend.Cell(r, 4).Value = m.PayrollCost;
            trend.Cell(r, 5).Value = m.Profit;
            r++;
        }

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] BuildPdf(string title, Action<ColumnDescriptor> configure)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Header().Text(title).Bold().FontSize(16);
                page.Content().PaddingTop(10).Column(configure);
                page.Footer().AlignCenter().Text(x => { x.Span("Page "); x.CurrentPageNumber(); });
            });
        });
        return doc.GeneratePdf();
    }

    private static void AddSection(ColumnDescriptor col, string title) =>
        col.Item().PaddingTop(8).Text(title).Bold().FontSize(11);
}
