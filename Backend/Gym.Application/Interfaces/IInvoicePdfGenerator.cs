using Gym.Application.DTOs.Payments;

namespace Gym.Application.Interfaces;

public interface IInvoicePdfGenerator
{
    byte[] Generate(InvoiceDto invoice);
}
