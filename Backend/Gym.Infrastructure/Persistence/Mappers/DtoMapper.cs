using Gym.Application.DTOs.Dashboard;
using Gym.Application.DTOs.Gyms;
using Gym.Application.DTOs.Members;
using Gym.Application.DTOs.Memberships;
using Gym.Application.DTOs.Payments;
using Gym.Application.DTOs.Trainers;
using Gym.Infrastructure.Persistence.Models;

namespace Gym.Infrastructure.Persistence.Mappers;

internal static class DtoMapper
{
    public static GymDto ToGymDto(GymRow row) =>
        new()
        {
            Id = row.GymId,
            Name = row.Name,
            Address = row.Address,
            Phone = row.Phone,
            Email = row.Email,
            LogoUrl = row.LogoUrl,
            IsActive = row.IsActive,
            CreatedAt = row.CreatedAt
        };

    public static TrainerDto ToTrainerDto(TrainerRow row) =>
        new()
        {
            Id = row.TrainerId,
            GymId = row.GymId,
            UserId = row.UserId,
            FullName = row.UserName,
            Email = row.UserEmail,
            Specialization = row.Specialization,
            Bio = row.Bio,
            IsActive = row.IsActive,
            AssignedMemberCount = row.AssignedMemberCount,
            CreatedAt = row.CreatedAt,
            UpdatedAt = row.UpdatedAt
        };

    public static MemberDto ToMemberDto(MemberRow row) =>
        new()
        {
            Id = row.MemberId,
            GymId = row.GymId,
            UserId = row.UserId,
            FullName = string.IsNullOrEmpty(row.FullName) ? row.UserName ?? string.Empty : row.FullName,
            LoginIdentifier = row.LoginIdentifier,
            Email = string.IsNullOrEmpty(row.Email) ? row.UserEmail ?? string.Empty : row.Email,
            TrainerId = row.TrainerId,
            TrainerName = row.TrainerName,
            DateOfBirth = row.DateOfBirth,
            Age = row.Age,
            Gender = row.Gender,
            Height = row.Height,
            Weight = row.Weight,
            Phone = row.Phone,
            Address = row.Address,
            EmergencyContact = row.EmergencyContact,
            JoinDate = row.JoinDate,
            IsActive = row.IsActive,
            IsDeleted = row.IsDeleted,
            MembershipStatus = row.MembershipStatus,
            MembershipPlanName = row.MembershipPlanName,
            MembershipEndDate = row.MembershipEndDate,
            CreatedDate = row.CreatedDate,
            UpdatedDate = row.UpdatedDate
        };

    public static MembershipResponseDto ToMembershipDto(MembershipRow row) =>
        new()
        {
            Id = row.MembershipId,
            GymId = row.GymId,
            MemberId = row.MemberId,
            MemberName = row.MemberName,
            MemberEmail = row.MemberEmail,
            MembershipPlanId = row.MembershipPlanId,
            PlanName = row.PlanName,
            PlanPrice = row.PlanPrice,
            DurationInMonths = row.DurationInMonths,
            StartDate = row.StartDate,
            EndDate = row.EndDate,
            Amount = row.Amount,
            Status = row.Status,
            Notes = row.Notes
        };

    public static MembershipPlanResponseDto ToMembershipPlanDto(MembershipPlanRow row) =>
        new()
        {
            Id = row.MembershipPlanId,
            GymId = row.GymId,
            PlanName = row.PlanName,
            Description = row.Description,
            DurationInMonths = row.DurationInMonths,
            Price = row.Price,
            IsActive = row.IsActive
        };

    public static PaymentResponseDto ToPaymentDto(PaymentRow row) =>
        new()
        {
            Id = row.PaymentId,
            GymId = row.GymId,
            MemberId = row.MemberId,
            MemberName = row.MemberName,
            MemberEmail = row.MemberEmail,
            MembershipId = row.MembershipId,
            MembershipPlanName = row.MembershipPlanName,
            Amount = row.Amount,
            PaymentDate = row.PaymentDate,
            PaymentMethod = row.PaymentMethod,
            TransactionReference = row.TransactionReference,
            RazorpayOrderId = row.RazorpayOrderId,
            RazorpayPaymentId = row.RazorpayPaymentId,
            Status = row.Status,
            Notes = row.Notes
        };

    public static DashboardStatsDto ToDashboardDto(DashboardStatsRow row) =>
        new()
        {
            TotalGyms = row.TotalGyms,
            ActiveGyms = row.ActiveGyms,
            TotalMembers = row.TotalMembers,
            ActiveMembers = row.ActiveMembers,
            MembersWithTrainer = row.MembersWithTrainer,
            TotalRevenue = row.TotalRevenue,
            ExpiredMemberships = row.ExpiredMemberships,
            ActiveMemberships = row.ActiveMemberships,
            PendingRenewals = row.PendingRenewals,
            MonthlyRevenue = row.MonthlyRevenue,
            TotalTrainers = row.TotalTrainers
        };
}
