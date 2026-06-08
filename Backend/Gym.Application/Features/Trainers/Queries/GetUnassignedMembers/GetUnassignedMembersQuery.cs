using Gym.Application.DTOs.Members;
using MediatR;

namespace Gym.Application.Features.Trainers.Queries.GetUnassignedMembers;

public record GetUnassignedMembersQuery(int TrainerId, string? Search) : IRequest<IReadOnlyList<MemberDto>>;
