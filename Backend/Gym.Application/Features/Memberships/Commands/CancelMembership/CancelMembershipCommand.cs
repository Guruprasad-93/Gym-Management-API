using Gym.Application.DTOs.Memberships;
using MediatR;

namespace Gym.Application.Features.Memberships.Commands.CancelMembership;

public record CancelMembershipCommand(int MembershipId, CancelMembershipDto Dto) : IRequest<Unit>;
