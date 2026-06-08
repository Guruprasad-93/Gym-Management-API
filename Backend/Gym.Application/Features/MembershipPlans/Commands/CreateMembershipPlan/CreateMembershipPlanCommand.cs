using Gym.Application.DTOs.Memberships;
using MediatR;

namespace Gym.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;

public record CreateMembershipPlanCommand(CreateMembershipPlanDto Dto) : IRequest<MembershipPlanResponseDto>;
