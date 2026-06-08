using Gym.Application.DTOs.Memberships;
using MediatR;

namespace Gym.Application.Features.Memberships.Commands.RenewMembership;

public record RenewMembershipCommand(int MembershipId, RenewMembershipDto Dto) : IRequest<MembershipResponseDto>;
