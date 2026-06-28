using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Payments;
using Gym.Application.DTOs.Notifications;
using Gym.Application.Interfaces;
using Gym.Application.Options;
using Gym.Application.Payments;
using Gym.Domain.Constants;
using Microsoft.Extensions.Options;

namespace Gym.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IRazorpayGateway _razorpayGateway;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly RazorpaySettings _razorpaySettings;
    private readonly INotificationService _notificationService;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IMembershipRepository membershipRepository,
        IMemberRepository memberRepository,
        IRazorpayGateway razorpayGateway,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IOptions<RazorpaySettings> razorpaySettings,
        INotificationService notificationService)
    {
        _paymentRepository = paymentRepository;
        _membershipRepository = membershipRepository;
        _memberRepository = memberRepository;
        _razorpayGateway = razorpayGateway;
        _currentUser = currentUser;
        _auditService = auditService;
        _razorpaySettings = razorpaySettings.Value;
        _notificationService = notificationService;
    }

    public async Task<PaymentResponseDto> CreateAsync(CreatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        var membership = await _membershipRepository.GetByIdAsync(dto.MembershipId, ResolveGymScope(), cancellationToken)
            ?? throw new KeyNotFoundException("Membership not found.");

        var gymId = ResolveGymIdForEntity(membership.GymId);
        var created = await _paymentRepository.CreateAsync(gymId, dto, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Payment,
            EntityId = created.Id.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = created
        }, cancellationToken);
        return created;
    }

    public Task<IReadOnlyList<PaymentResponseDto>> GetHistoryAsync(string? search, CancellationToken cancellationToken = default) =>
        _paymentRepository.GetHistoryAsync(ResolveGymScope(), search, cancellationToken);

    public Task<IReadOnlyList<PaymentResponseDto>> GetByMemberAsync(int memberId, CancellationToken cancellationToken = default) =>
        _paymentRepository.GetByMemberAsync(memberId, ResolveGymScope(), cancellationToken);

    public async Task<InvoiceDto> GenerateInvoiceAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        var gymId = await ResolveGymIdForPaymentAsync(paymentId, cancellationToken);
        return await _paymentRepository.GenerateInvoiceAsync(paymentId, gymId, cancellationToken);
    }

    public async Task<InvoiceDto> GetInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default) =>
        await _paymentRepository.GetInvoiceByIdAsync(invoiceId, ResolveGymScope(), cancellationToken)
        ?? throw new KeyNotFoundException("Invoice not found.");

    public Task<RevenueDashboardDto> GetRevenueDashboardAsync(Guid? gymId = null, CancellationToken cancellationToken = default) =>
        _paymentRepository.GetRevenueDashboardAsync(ResolveGymScope(gymId), cancellationToken);

    public Task<IReadOnlyList<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int months, CancellationToken cancellationToken = default) =>
        _paymentRepository.GetMonthlyRevenueAsync(ResolveGymScope(), months, cancellationToken);

    public async Task<RazorpayOrderResponseDto> CreateRazorpayOrderAsync(
        CreateRazorpayOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!_razorpaySettings.Enabled)
            throw new InvalidOperationException("Razorpay payments are not enabled.");

        var membership = await _membershipRepository.GetByIdAsync(dto.MembershipId, ResolveGymScope(), cancellationToken)
            ?? throw new KeyNotFoundException("Membership not found.");

        var gymId = ResolveGymIdForEntity(membership.GymId);
        var memberId = await ResolveMemberIdForOrderAsync(membership.MemberId, dto.MemberId, cancellationToken);
        if (membership.MemberId != memberId)
            throw new UnauthorizedAccessException("Membership does not belong to the specified member.");

        var amount = membership.Amount ?? membership.PlanPrice;
        var member = await _memberRepository.GetByIdAsync(memberId, gymId, null, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");

        var razorpayOrderId = await _razorpayGateway.CreateOrderAsync(
            amount,
            _razorpaySettings.Currency,
            $"gym_{membership.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}",
            new Dictionary<string, string>
            {
                ["membershipId"] = membership.Id.ToString(),
                ["memberId"] = memberId.ToString(),
                ["gymId"] = gymId.ToString()
            },
            cancellationToken);

        var payment = await _paymentRepository.CreateRazorpayOrderAsync(
            gymId,
            memberId,
            dto.MembershipId,
            razorpayOrderId,
            amount,
            dto.Notes,
            cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Payment,
            EntityId = payment.Id.ToString(),
            ActionType = AuditActionTypes.Create,
            NewValue = payment
        }, cancellationToken);

        return BuildRazorpayOrderResponse(payment.Id, razorpayOrderId, amount, member.FullName, member.Email, membership.PlanName);
    }

    private RazorpayOrderResponseDto BuildRazorpayOrderResponse(
        int paymentId,
        string razorpayOrderId,
        decimal amount,
        string? memberName,
        string? memberEmail,
        string? planName)
    {
        var response = new RazorpayOrderResponseDto
        {
            PaymentId = paymentId,
            RazorpayOrderId = razorpayOrderId,
            KeyId = _razorpaySettings.KeyId,
            Amount = amount,
            Currency = _razorpaySettings.Currency,
            AmountInPaise = (int)Math.Round(amount * 100m, MidpointRounding.AwayFromZero),
            MemberName = memberName,
            MemberEmail = memberEmail,
            PlanName = planName
        };

        if (_razorpaySettings.UseMockGateway || RazorpayMockCheckoutHelper.IsMockOrder(razorpayOrderId))
        {
            var mock = RazorpayMockCheckoutHelper.CreateSuccess(razorpayOrderId, _razorpaySettings.KeySecret);
            response.UseMockCheckout = true;
            response.MockPaymentId = mock.PaymentId;
            response.MockSignature = mock.Signature;
        }

        return response;
    }

    public async Task<PaymentResponseDto> VerifyRazorpayPaymentAsync(
        VerifyRazorpayPaymentDto dto,
        CancellationToken cancellationToken = default)
    {
        var pending = await _paymentRepository.GetByRazorpayOrderIdAsync(dto.RazorpayOrderId, ResolveGymScope(), cancellationToken)
            ?? throw new KeyNotFoundException("Payment not found.");

        var gymId = ResolveGymIdForEntity(pending.GymId);
        await EnsureCanAccessPaymentMemberAsync(pending.MemberId, cancellationToken);

        if (!_razorpayGateway.VerifyPaymentSignature(dto.RazorpayOrderId, dto.RazorpayPaymentId, dto.RazorpaySignature))
        {
            await _paymentRepository.FailRazorpayPaymentAsync(gymId, dto.RazorpayOrderId, "Signature verification failed.", cancellationToken);
            var failed = await _paymentRepository.GetByRazorpayOrderIdAsync(dto.RazorpayOrderId, gymId, cancellationToken);
            await _auditService.LogAsync(new AuditLogEntryDto
            {
                GymId = gymId,
                EntityName = AuditEntityNames.Payment,
                EntityId = pending.Id.ToString(),
                ActionType = AuditActionTypes.PaymentFailed,
                OldValue = pending,
                NewValue = failed
            }, cancellationToken);
            throw new UnauthorizedAccessException("Invalid Razorpay payment signature.");
        }

        var (confirmed, newMembershipId) = await _paymentRepository.ConfirmRazorpayPaymentAsync(
            gymId,
            dto.RazorpayOrderId,
            dto.RazorpayPaymentId,
            renewMembership: true,
            cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Payment,
            EntityId = confirmed.Id.ToString(),
            ActionType = AuditActionTypes.Update,
            OldValue = pending,
            NewValue = confirmed
        }, cancellationToken);

        if (newMembershipId.HasValue && newMembershipId != pending.MembershipId)
        {
            await _auditService.LogAsync(new AuditLogEntryDto
            {
                GymId = gymId,
                EntityName = AuditEntityNames.Membership,
                EntityId = newMembershipId.Value.ToString(),
                ActionType = AuditActionTypes.Renew,
                NewValue = new { membershipId = newMembershipId.Value, paymentId = confirmed.Id }
            }, cancellationToken);
        }

        await _paymentRepository.GenerateInvoiceAsync(confirmed.Id, gymId, cancellationToken);

        if (confirmed.MemberId.HasValue)
        {
            var member = await _memberRepository.GetByIdAsync(confirmed.MemberId.Value, gymId, null, cancellationToken);
            if (member?.Phone is { Length: > 0 } phone)
            {
                await _notificationService.SendEventNotificationAsync(gymId, new SendNotificationRequestDto
                {
                    NotificationType = NotificationTypes.PaymentSuccess,
                    PhoneNumber = phone,
                    RecipientUserId = member.UserId,
                    MemberId = member.Id,
                    Variables = new Dictionary<string, string>
                    {
                        ["memberName"] = member.FullName,
                        ["amount"] = confirmed.Amount.ToString("0.00"),
                        ["paymentMethod"] = confirmed.PaymentMethod
                    },
                    RelatedEntityType = AuditEntityNames.Payment,
                    RelatedEntityId = confirmed.Id.ToString()
                }, cancellationToken);

                if (newMembershipId.HasValue)
                {
                    await _notificationService.SendEventNotificationAsync(gymId, new SendNotificationRequestDto
                    {
                        NotificationType = NotificationTypes.MembershipRenewal,
                        PhoneNumber = phone,
                        RecipientUserId = member.UserId,
                        MemberId = member.Id,
                        Variables = new Dictionary<string, string>
                        {
                            ["memberName"] = member.FullName,
                            ["membershipId"] = newMembershipId.Value.ToString()
                        },
                        RelatedEntityType = AuditEntityNames.Membership,
                        RelatedEntityId = newMembershipId.Value.ToString()
                    }, cancellationToken);
                }
            }
        }

        return confirmed;
    }

    public async Task<RazorpayCheckoutContextDto?> GetCheckoutContextAsync(CancellationToken cancellationToken = default)
    {
        var member = await _memberRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new KeyNotFoundException("Member profile not found.");

        var gymId = ResolveGymIdForEntity(member.GymId);
        return await _paymentRepository.GetMemberPayableMembershipAsync(member.Id, gymId, cancellationToken);
    }

    public async Task<RefundPaymentResponseDto> RefundPaymentAsync(
        int paymentId,
        RefundPaymentDto dto,
        CancellationToken cancellationToken = default)
    {
        var gymId = await ResolveGymIdForPaymentAsync(paymentId, cancellationToken);
        var payments = await _paymentRepository.GetHistoryAsync(gymId, null, cancellationToken);
        var existing = payments.FirstOrDefault(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        if (string.IsNullOrWhiteSpace(existing.RazorpayPaymentId))
            throw new InvalidOperationException("Payment does not have a Razorpay payment id.");

        var refundReference = await _razorpayGateway.RefundPaymentAsync(
            existing.RazorpayPaymentId,
            dto.Amount,
            cancellationToken);

        var refunded = await _paymentRepository.RefundPaymentAsync(
            paymentId,
            gymId,
            refundReference,
            dto.Reason,
            cancellationToken);

        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId,
            EntityName = AuditEntityNames.Payment,
            EntityId = paymentId.ToString(),
            ActionType = AuditActionTypes.Refund,
            OldValue = existing,
            NewValue = refunded
        }, cancellationToken);

        return new RefundPaymentResponseDto
        {
            PaymentId = paymentId,
            Status = refunded.Status,
            RefundReference = refundReference
        };
    }

    private async Task<int> ResolveMemberIdForOrderAsync(
        int membershipMemberId,
        int? requestedMemberId,
        CancellationToken cancellationToken)
    {
        if (_currentUser.HasRole(RoleNames.Member))
        {
            var own = await _memberRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
                ?? throw new UnauthorizedAccessException("Member profile not found.");
            return own.Id;
        }

        if (!requestedMemberId.HasValue)
            throw new ArgumentException("MemberId is required.");

        return requestedMemberId.Value;
    }

    private async Task EnsureCanAccessPaymentMemberAsync(int? memberId, CancellationToken cancellationToken)
    {
        if (!_currentUser.HasRole(RoleNames.Member) || memberId is null)
            return;

        var own = await _memberRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Member profile not found.");

        if (own.Id != memberId)
            throw new UnauthorizedAccessException("Payment does not belong to the current member.");
    }

    private async Task<Guid> ResolveGymIdForPaymentAsync(int paymentId, CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetHistoryAsync(ResolveGymScope(), null, cancellationToken);
        var payment = payments.FirstOrDefault(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        return ResolveGymIdForEntity(payment.GymId);
    }

    private Guid ResolveGymIdForEntity(Guid entityGymId) =>
        GymScopeResolver.ResolveForEntity(_currentUser, entityGymId);

    private Guid ResolveGymScope(Guid? requestedGymId = null) =>
        GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);
}
