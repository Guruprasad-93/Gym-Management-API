using Gym.Application.DTOs.Memberships;
using MediatR;

namespace Gym.Application.Features.Memberships.Commands.CreateMembership;

public record CreateMembershipCommand(CreateMembershipDto Dto) : IRequest<MembershipResponseDto>;
