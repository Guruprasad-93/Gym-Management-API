using Gym.Application.DTOs.Payments;
using Gym.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gym.Infrastructure.Services;

public class InvoicePdfGenerator : IInvoicePdfGenerator
{
    static InvoicePdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(InvoiceDto invoice)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(column =>
                {
                    column.Item().Text("INVOICE").Bold().FontSize(22).FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"#{invoice.InvoiceNumber}").FontSize(14).SemiBold();
                    column.Item().PaddingTop(4).Text($"Issued: {invoice.IssuedAt:yyyy-MM-dd HH:mm} UTC").FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(20).Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("From").Bold().Underline();
                            left.Item().Text(invoice.GymName);
                            if (!string.IsNullOrWhiteSpace(invoice.GymAddress))
                                left.Item().Text(invoice.GymAddress);
                            if (!string.IsNullOrWhiteSpace(invoice.GymPhone))
                                left.Item().Text($"Phone: {invoice.GymPhone}");
                            if (!string.IsNullOrWhiteSpace(invoice.GymEmail))
                                left.Item().Text($"Email: {invoice.GymEmail}");
                        });

                        row.RelativeItem().Column(right =>
                        {
                            right.Item().Text("Bill To").Bold().Underline();
                            right.Item().Text(invoice.MemberName);
                            right.Item().Text(invoice.MemberEmail);
                            if (!string.IsNullOrWhiteSpace(invoice.MemberPhone))
                                right.Item().Text($"Phone: {invoice.MemberPhone}");
                        });
                    });

                    column.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });

                        void Row(string label, string value)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                .Text(label).SemiBold();
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                .Text(value);
                        }

                        Row("Membership Plan", invoice.MembershipPlanName ?? "N/A");
                        Row("Payment Date", invoice.PaymentDate.ToString("yyyy-MM-dd"));
                        Row("Payment Method", invoice.PaymentMethod);
                        if (!string.IsNullOrWhiteSpace(invoice.TransactionReference))
                            Row("Reference", invoice.TransactionReference);
                        Row("Amount Paid", invoice.Amount.ToString("C"));
                        if (!string.IsNullOrWhiteSpace(invoice.PaymentNotes))
                            Row("Notes", invoice.PaymentNotes);
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Thank you for your business — ");
                    text.Span(invoice.GymName).SemiBold();
                });
            });
        });

        return document.GeneratePdf();
    }
}
