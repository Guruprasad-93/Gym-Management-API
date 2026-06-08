using Gym.Application.DTOs.Memberships;
using MediatR;

namespace Gym.Application.Features.Memberships.Queries.GetMembershipById;

public record GetMembershipByIdQuery(int MembershipId) : IRequest<MembershipResponseDto>;
