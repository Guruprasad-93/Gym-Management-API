namespace Gym.Application.DTOs.Payments;

public class CreatePaymentDto
{
    public int MemberId { get; set; }
    public int MembershipId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string PaymentMethod { get; set; } = "Cash";
    public string? TransactionReference { get; set; }
    public string? Notes { get; set; }
}
