namespace Gym.Infrastructure.Persistence.Models;

internal sealed class PaymentRow
{
    public int PaymentId { get; set; }
    public Guid GymId { get; set; }
    public int? MemberId { get; set; }
    public int? MembershipId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public string? RazorpayOrderId { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? MemberName { get; set; }
    public string? MemberEmail { get; set; }
    public string? MembershipPlanName { get; set; }
}
