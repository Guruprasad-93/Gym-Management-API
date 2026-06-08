using Gym.Domain.Enums;

namespace Gym.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid GymId { get; private set; }
    public int? MemberId { get; private set; }
    public int? MembershipId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public string? TransactionReference { get; private set; }
    public string Status { get; private set; } = PaymentStatus.Completed;
    public string? Notes { get; private set; }

    public Gym Gym { get; private set; } = null!;
    public Member? Member { get; private set; }
    public Membership? Membership { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid gymId,
        decimal amount,
        string paymentMethod,
        DateTime paymentDate,
        int? memberId = null,
        int? membershipId = null) =>
        new()
        {
            GymId = gymId,
            MemberId = memberId,
            MembershipId = membershipId,
            Amount = amount,
            PaymentMethod = paymentMethod.Trim(),
            PaymentDate = paymentDate,
            CreatedAt = DateTime.UtcNow
        };
}
