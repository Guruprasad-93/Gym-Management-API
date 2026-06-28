namespace Gym.Application.DTOs.Payments;

public class CreateRazorpayOrderDto
{
    public int MembershipId { get; set; }
    public int? MemberId { get; set; }
    public bool RenewOnSuccess { get; set; } = true;
    public string? Notes { get; set; }
}

public class RazorpayOrderResponseDto
{
    public int PaymentId { get; set; }
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string KeyId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public int AmountInPaise { get; set; }
    public string? MemberName { get; set; }
    public string? MemberEmail { get; set; }
    public string? PlanName { get; set; }
    public bool UseMockCheckout { get; set; }
    public string? MockPaymentId { get; set; }
    public string? MockSignature { get; set; }
}

public class VerifyRazorpayPaymentDto
{
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
}

public class RazorpayCheckoutContextDto
{
    public int MemberId { get; set; }
    public int MembershipId { get; set; }
    public decimal Amount { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly? EndDate { get; set; }
    public bool IsExpired { get; set; }
}

public class RefundPaymentDto
{
    public decimal? Amount { get; set; }
    public string? Reason { get; set; }
}

public class RefundPaymentResponseDto
{
    public int PaymentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RefundReference { get; set; }
}
