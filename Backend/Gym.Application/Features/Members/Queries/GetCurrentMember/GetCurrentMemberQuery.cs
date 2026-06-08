using Gym.Application.DTOs.Members;
using MediatR;

namespace Gym.Application.Features.Members.Queries.GetCurrentMember;

public record GetCurrentMemberQuery : IRequest<MemberResponseDto>;
