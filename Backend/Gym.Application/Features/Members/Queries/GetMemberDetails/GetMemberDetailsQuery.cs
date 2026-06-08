using Gym.Application.DTOs.Members;
using MediatR;

namespace Gym.Application.Features.Members.Queries.GetMemberDetails;

public record GetMemberDetailsQuery(int MemberId) : IRequest<MemberDetailsDto>;
