using Gym.Application.DTOs.Memberships;
using MediatR;

namespace Gym.Application.Features.MembershipPlans.Commands.UpdateMembershipPlan;

public record UpdateMembershipPlanCommand(int PlanId, UpdateMembershipPlanDto Dto) : IRequest<MembershipPlanResponseDto>;
