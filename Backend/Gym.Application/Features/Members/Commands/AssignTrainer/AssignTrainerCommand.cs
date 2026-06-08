using Gym.Application.DTOs.Members;
using MediatR;

namespace Gym.Application.Features.Members.Commands.AssignTrainer;

public record AssignTrainerCommand(int MemberId, AssignTrainerToMemberDto Dto) : IRequest<Unit>;
