using MediatR;

namespace Gym.Application.Features.GymAdmins.Commands.SetGymAdminActive;

public record SetGymAdminActiveCommand(Guid UserId, bool IsActive) : IRequest;
