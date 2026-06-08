using Gym.Application.DTOs.Gyms;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Gyms.Queries.GetAllGyms;

public class GetAllGymsQueryHandler : IRequestHandler<GetAllGymsQuery, IReadOnlyList<GymDto>>
{
    private readonly IGymService _gymService;

    public GetAllGymsQueryHandler(IGymService gymService) => _gymService = gymService;

    public Task<IReadOnlyList<GymDto>> Handle(GetAllGymsQuery request, CancellationToken cancellationToken) =>
        _gymService.GetAllAsync(cancellationToken);
}
