using MediatR;

namespace Gym.Application.Features.MembershipPlans.Commands.DeleteMembershipPlan;

public record DeleteMembershipPlanCommand(int PlanId) : IRequest<Unit>;
