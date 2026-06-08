namespace Gym.Application.DTOs.Payments;

public class InvoiceDto
{
    public int Id { get; set; }
    public Guid GymId { get; set; }
    public int PaymentId { get; set; }
    public int MemberId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime IssuedAt { get; set; }
    public string GymName { get; set; } = string.Empty;
    public string? GymAddress { get; set; }
    public string? GymPhone { get; set; }
    public string? GymEmail { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public string? MemberPhone { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public string? PaymentNotes { get; set; }
    public string? MembershipPlanName { get; set; }
}
