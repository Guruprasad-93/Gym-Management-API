namespace Gym.Application.DTOs.Members;

public class MemberDetailsDto : MemberResponseDto
{
    public IReadOnlyList<MemberPaymentHistoryDto> PaymentHistory { get; set; } = Array.Empty<MemberPaymentHistoryDto>();
    public IReadOnlyList<MemberProgressDto> Progress { get; set; } = Array.Empty<MemberProgressDto>();
}
