using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using BillingService.DTOs.Invoice;

namespace BillingService.Services;

public class PdfService
{
    public byte[] GenerateInvoicePdf(InvoiceDTO invoice)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);

                page.Content().Column(col =>
                {
                    col.Item().Text($"Invoice #{invoice.BillId}")
                        .FontSize(20).Bold();

                    col.Item().Text(invoice.StoreName).SemiBold();
                    col.Item().Text(invoice.StoreAddress);
                    col.Item().Text($"Invoice No: {invoice.InvoiceNumber}");
                    col.Item().Text($"Date: {invoice.CreatedAt:dd MMM yyyy, hh:mm tt}");
                    col.Item().Text($"User ID: {invoice.UserId}");
                    col.Item().Text($"Payment: {invoice.PaymentMethod} ({invoice.PaymentStatus})");

                    col.Item().LineHorizontal(1);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Product");
                            header.Cell().Text("Barcode");
                            header.Cell().Text("Qty");
                            header.Cell().Text("Price");
                            header.Cell().Text("Tax");
                            header.Cell().Text("Total");
                        });

                        foreach (var item in invoice.Items ?? Enumerable.Empty<InvoiceItemDTO>())
                        {
                            table.Cell().Text(item.ProductName);
                            table.Cell().Text(item.Barcode);
                            table.Cell().Text(item.Quantity.ToString());
                            table.Cell().Text(item.Price.ToString("0.00"));
                            table.Cell().Text(item.TaxAmount.ToString("0.00"));
                            table.Cell().Text(item.LineTotal.ToString("0.00"));
                        }
                    });

                    col.Item().LineHorizontal(1);

                    col.Item().Text($"Subtotal: {invoice.SubtotalAmount:0.00}");
                    col.Item().Text($"Tax: {invoice.TaxAmount:0.00}");
                    col.Item().Text($"Discount: {invoice.DiscountAmount:0.00}");
                    col.Item().Text($"Total: {invoice.TotalAmount}")
                        .Bold();
                });
            });
        });

        return document.GeneratePdf();
    }
}
