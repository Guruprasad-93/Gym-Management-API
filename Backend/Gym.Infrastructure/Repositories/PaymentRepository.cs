using System.Data;
using Dapper;
using Gym.Application.DTOs.Payments;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Mappers;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public PaymentRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<PaymentResponseDto> CreateAsync(Guid gymId, CreatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", dto.MemberId);
        parameters.Add("@MembershipId", dto.MembershipId);
        parameters.Add("@Amount", dto.Amount);
        parameters.Add("@PaymentDate", dto.PaymentDate);
        parameters.Add("@PaymentMethod", dto.PaymentMethod);
        parameters.Add("@TransactionReference", dto.TransactionReference);
        parameters.Add("@Notes", dto.Notes);
        parameters.Add("@PaymentId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var paymentId = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.CreatePayment, parameters, "@PaymentId", cancellationToken);

        var rows = await _sp.QueryAsync<PaymentRow>(
            StoredProcedureNames.GetPaymentHistory, new { GymId = gymId }, cancellationToken);

        return DtoMapper.ToPaymentDto(rows.First(p => p.PaymentId == paymentId));
    }

    public async Task<IReadOnlyList<PaymentResponseDto>> GetHistoryAsync(
        Guid? gymId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<PaymentRow>(
            StoredProcedureNames.GetPaymentHistory,
            new { GymId = gymId, Search = search },
            cancellationToken);

        return rows.Select(DtoMapper.ToPaymentDto).ToList();
    }

    public async Task<IReadOnlyList<PaymentResponseDto>> GetByMemberAsync(
        int memberId,
        Guid? gymId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<PaymentRow>(
            StoredProcedureNames.GetPaymentsByMember,
            new { MemberId = memberId, GymId = gymId },
            cancellationToken);

        return rows.Select(DtoMapper.ToPaymentDto).ToList();
    }

    public async Task<InvoiceDto> GenerateInvoiceAsync(int paymentId, Guid gymId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@PaymentId", paymentId);
        parameters.Add("@GymId", gymId);
        parameters.Add("@InvoiceId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        parameters.Add("@InvoiceNumber", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);

        await _sp.ExecuteAsync(StoredProcedureNames.GenerateInvoice, parameters, cancellationToken);

        var invoiceId = parameters.Get<int>("@InvoiceId");
        return (await GetInvoiceByIdAsync(invoiceId, gymId, cancellationToken))!;
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(int invoiceId, Guid? gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<InvoiceRow>(
            StoredProcedureNames.GetInvoiceById,
            new { InvoiceId = invoiceId, GymId = gymId },
            cancellationToken);

        return row is null ? null : MapInvoice(row);
    }

    public async Task<RevenueDashboardDto> GetRevenueDashboardAsync(Guid? gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<RevenueDashboardRow>(
            StoredProcedureNames.GetRevenueDashboard,
            new { GymId = gymId },
            cancellationToken);

        return new RevenueDashboardDto
        {
            TotalRevenue = row?.TotalRevenue ?? 0,
            MonthlyRevenue = row?.MonthlyRevenue ?? 0,
            ExpiredMemberships = row?.ExpiredMemberships ?? 0,
            ActiveMemberships = row?.ActiveMemberships ?? 0,
            PendingRenewals = row?.PendingRenewals ?? 0
        };
    }

    public async Task<IReadOnlyList<MonthlyRevenueDto>> GetMonthlyRevenueAsync(
        Guid? gymId,
        int months,
        CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<MonthlyRevenueRow>(
            StoredProcedureNames.GetMonthlyRevenueSummary,
            new { GymId = gymId, Months = months },
            cancellationToken);

        return rows.Select(r => new MonthlyRevenueDto
        {
            Year = r.Year,
            Month = r.Month,
            MonthLabel = r.MonthLabel,
            Revenue = r.Revenue
        }).ToList();
    }

    public async Task<PaymentResponseDto> CreateRazorpayOrderAsync(
        Guid gymId,
        int memberId,
        int membershipId,
        string razorpayOrderId,
        decimal amount,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@MemberId", memberId);
        parameters.Add("@MembershipId", membershipId);
        parameters.Add("@RazorpayOrderId", razorpayOrderId);
        parameters.Add("@Amount", amount);
        parameters.Add("@Notes", notes);
        parameters.Add("@PaymentId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var paymentId = await _sp.ExecuteWithOutputAsync<int>(
            StoredProcedureNames.CreateRazorpayPaymentOrder, parameters, "@PaymentId", cancellationToken);

        return (await GetByIdFromHistoryAsync(paymentId, gymId, cancellationToken))!;
    }

    public async Task<PaymentResponseDto?> GetByRazorpayOrderIdAsync(
        string razorpayOrderId,
        Guid? gymId,
        CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<PaymentRow>(
            StoredProcedureNames.GetPaymentByRazorpayOrderId,
            new { RazorpayOrderId = razorpayOrderId, GymId = gymId },
            cancellationToken);

        return row is null ? null : DtoMapper.ToPaymentDto(row);
    }

    public async Task<(PaymentResponseDto Payment, int? NewMembershipId)> ConfirmRazorpayPaymentAsync(
        Guid gymId,
        string razorpayOrderId,
        string razorpayPaymentId,
        bool renewMembership,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@RazorpayOrderId", razorpayOrderId);
        parameters.Add("@RazorpayPaymentId", razorpayPaymentId);
        parameters.Add("@RenewMembership", renewMembership);
        parameters.Add("@PaymentId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        parameters.Add("@NewMembershipId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await _sp.ExecuteAsync(StoredProcedureNames.ConfirmRazorpayPayment, parameters, cancellationToken);

        var paymentId = parameters.Get<int>("@PaymentId");
        var newMembershipId = parameters.Get<int?>("@NewMembershipId");
        var payment = (await GetByIdFromHistoryAsync(paymentId, gymId, cancellationToken))!;
        return (payment, newMembershipId);
    }

    public async Task FailRazorpayPaymentAsync(
        Guid gymId,
        string razorpayOrderId,
        string? failureReason,
        CancellationToken cancellationToken = default) =>
        await _sp.ExecuteAsync(
            StoredProcedureNames.FailRazorpayPayment,
            new { GymId = gymId, RazorpayOrderId = razorpayOrderId, FailureReason = failureReason },
            cancellationToken);

    public async Task<PaymentResponseDto> RefundPaymentAsync(
        int paymentId,
        Guid gymId,
        string? refundReference,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        await _sp.ExecuteAsync(
            StoredProcedureNames.RefundPayment,
            new { PaymentId = paymentId, GymId = gymId, RefundReference = refundReference, Notes = notes },
            cancellationToken);

        return (await GetByIdFromHistoryAsync(paymentId, gymId, cancellationToken))!;
    }

    public async Task<RazorpayCheckoutContextDto?> GetMemberPayableMembershipAsync(
        int memberId,
        Guid gymId,
        CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<PayableMembershipRow>(
            StoredProcedureNames.GetMemberPayableMembership,
            new { MemberId = memberId, GymId = gymId },
            cancellationToken);

        if (row is null)
            return null;

        var endDate = DateOnly.FromDateTime(row.EndDate);
        return new RazorpayCheckoutContextDto
        {
            MemberId = row.MemberId,
            MembershipId = row.MembershipId,
            Amount = row.Amount,
            PlanName = row.PlanName,
            Status = row.Status,
            EndDate = endDate,
            IsExpired = endDate < DateOnly.FromDateTime(DateTime.UtcNow.Date)
        };
    }

    private async Task<PaymentResponseDto?> GetByIdFromHistoryAsync(
        int paymentId,
        Guid gymId,
        CancellationToken cancellationToken)
    {
        var rows = await _sp.QueryAsync<PaymentRow>(
            StoredProcedureNames.GetPaymentHistory,
            new { GymId = gymId },
            cancellationToken);

        var row = rows.FirstOrDefault(p => p.PaymentId == paymentId);
        return row is null ? null : DtoMapper.ToPaymentDto(row);
    }

    private static InvoiceDto MapInvoice(InvoiceRow row) => new()
    {
        Id = row.InvoiceId,
        GymId = row.GymId,
        PaymentId = row.PaymentId,
        MemberId = row.MemberId,
        InvoiceNumber = row.InvoiceNumber,
        Amount = row.Amount,
        IssuedAt = row.IssuedAt,
        GymName = row.GymName,
        GymAddress = row.GymAddress,
        GymPhone = row.GymPhone,
        GymEmail = row.GymEmail,
        MemberName = row.MemberName,
        MemberEmail = row.MemberEmail,
        MemberPhone = row.MemberPhone,
        PaymentDate = row.PaymentDate,
        PaymentMethod = row.PaymentMethod,
        TransactionReference = row.TransactionReference,
        PaymentNotes = row.PaymentNotes,
        MembershipPlanName = row.MembershipPlanName
    };
}
