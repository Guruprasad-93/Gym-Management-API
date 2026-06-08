using Gym.Application.DTOs.Payments;
using MediatR;

namespace Gym.Application.Features.Payments.Queries.GetMonthlyRevenue;

public record GetMonthlyRevenueQuery(int Months) : IRequest<IReadOnlyList<MonthlyRevenueDto>>;
