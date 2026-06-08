using Gym.Application.DTOs.Members;
using MediatR;

namespace Gym.Application.Features.Trainers.Queries.GetTrainerMembers;

public record GetTrainerMembersQuery(int TrainerId, string? Search) : IRequest<IReadOnlyList<MemberDto>>;
