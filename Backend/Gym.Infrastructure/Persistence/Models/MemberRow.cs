namespace Gym.Infrastructure.Persistence.Models;

internal sealed class MemberRow
{
    public int MemberId { get; set; }
    public Guid GymId { get; set; }
    public Guid UserId { get; set; }
    public int? TrainerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string LoginIdentifier { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public DateOnly JoinDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? TrainerName { get; set; }
    public string? MembershipStatus { get; set; }
    public string? MembershipPlanName { get; set; }
    public DateOnly? MembershipEndDate { get; set; }
}

internal sealed class MemberProgressRow
{
    public string ProgressType { get; set; } = string.Empty;
    public DateOnly RecordedDate { get; set; }
    public string? Detail { get; set; }
    public decimal? WeightKg { get; set; }
    public DateTime CreatedAt { get; set; }
}
