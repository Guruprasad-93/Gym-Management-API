using Gym.Application.DTOs.Members;
using MediatR;

namespace Gym.Application.Features.Members.Queries.GetMemberById;

public record GetMemberByIdQuery(int MemberId) : IRequest<MemberResponseDto>;
