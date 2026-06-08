using Gym.Application.DTOs.Gyms;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Gyms.Queries.GetGymById;

public class GetGymByIdQueryHandler : IRequestHandler<GetGymByIdQuery, GymDto>
{
    private readonly IGymService _gymService;

    public GetGymByIdQueryHandler(IGymService gymService) => _gymService = gymService;

    public async Task<GymDto> Handle(GetGymByIdQuery request, CancellationToken cancellationToken) =>
        await _gymService.GetByIdAsync(request.Id, cancellationToken)
        ?? throw new KeyNotFoundException("Gym not found.");
}
