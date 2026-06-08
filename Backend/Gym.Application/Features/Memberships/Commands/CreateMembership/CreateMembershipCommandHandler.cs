using Gym.Application.DTOs.Memberships;
using Gym.Application.Interfaces;
using MediatR;

namespace Gym.Application.Features.Memberships.Commands.CreateMembership;

public class CreateMembershipCommandHandler : IRequestHandler<CreateMembershipCommand, MembershipResponseDto>
{
    private readonly IMembershipService _membershipService;

    public CreateMembershipCommandHandler(IMembershipService membershipService) =>
        _membershipService = membershipService;

    public Task<MembershipResponseDto> Handle(CreateMembershipCommand request, CancellationToken cancellationToken) =>
        _membershipService.CreateAsync(request.Dto, cancellationToken);
}
