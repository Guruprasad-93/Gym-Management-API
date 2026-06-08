namespace Gym.Application.DTOs.Members;

public class MemberPaymentHistoryDto
{
    public int Id { get; set; }
    public int? MembershipId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly PaymentDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
