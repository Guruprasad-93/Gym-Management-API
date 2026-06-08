using System.Data;
using Dapper;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Financial;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence.Models;
using Gym.Infrastructure.StoredProcedures;

namespace Gym.Infrastructure.Repositories;

public class PayrollRepository : IPayrollRepository
{
    private readonly IStoredProcedureExecutor _sp;

    public PayrollRepository(IStoredProcedureExecutor sp) => _sp = sp;

    public async Task<PayrollDto?> GetByIdAsync(int payrollId, Guid gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<PayrollRow>(
            StoredProcedureNames.GetPayrollById, new { PayrollId = payrollId, GymId = gymId }, cancellationToken);
        return row is null ? null : MapPayroll(row);
    }

    public async Task<PagedResultDto<PayrollDto>> GetPagedAsync(Guid gymId, PayrollSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@SalaryMonth", query.SalaryMonth);
        parameters.Add("@Status", query.Status);
        parameters.Add("@EmployeeType", query.EmployeeType);
        parameters.Add("@PageNumber", query.PageNumber);
        parameters.Add("@PageSize", query.PageSize);
        parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rows = await _sp.QueryAsync<PayrollRow>(StoredProcedureNames.GetPayrollsPaged, parameters, cancellationToken);
        return new PagedResultDto<PayrollDto>
        {
            Items = rows.Select(MapPayroll).ToList(),
            TotalCount = parameters.Get<int>("@TotalCount"),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public Task UpdateAsync(int payrollId, Guid gymId, UpdatePayrollDto dto, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.UpdatePayroll, new
        {
            PayrollId = payrollId,
            GymId = gymId,
            dto.BaseSalary,
            dto.IncentiveAmount,
            dto.CommissionAmount,
            dto.DeductionAmount
        }, cancellationToken);

    public async Task<int> GenerateMonthlyAsync(
        Guid gymId, DateOnly salaryMonth, decimal trainerSalary, decimal staffSalary, Guid? createdBy, CancellationToken cancellationToken = default)
    {
        var monthStart = new DateOnly(salaryMonth.Year, salaryMonth.Month, 1);
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@SalaryMonth", monthStart);
        parameters.Add("@DefaultTrainerBaseSalary", trainerSalary);
        parameters.Add("@DefaultStaffBaseSalary", staffSalary);
        parameters.Add("@CreatedBy", createdBy);
        parameters.Add("@GeneratedCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.GenerateMonthlyPayroll, parameters, cancellationToken);
        return parameters.Get<int>("@GeneratedCount");
    }

    public Task ApproveAsync(int payrollId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.ApprovePayroll, new { PayrollId = payrollId, GymId = gymId }, cancellationToken);

    public Task PayAsync(int payrollId, Guid gymId, CancellationToken cancellationToken = default) =>
        _sp.ExecuteAsync(StoredProcedureNames.PayPayroll, new { PayrollId = payrollId, GymId = gymId }, cancellationToken);

    public async Task<int> CreateCommissionAsync(Guid gymId, Guid? createdBy, CreateTrainerCommissionDto dto, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@GymId", gymId);
        parameters.Add("@TrainerId", dto.TrainerId);
        parameters.Add("@MemberId", dto.MemberId);
        parameters.Add("@PaymentId", dto.PaymentId);
        parameters.Add("@Amount", dto.Amount);
        parameters.Add("@CreatedBy", createdBy);
        parameters.Add("@CommissionId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        await _sp.ExecuteAsync(StoredProcedureNames.CreateTrainerCommission, parameters, cancellationToken);
        return parameters.Get<int>("@CommissionId");
    }

    public async Task<IReadOnlyList<TrainerCommissionDto>> GetCommissionReportAsync(
        Guid? gymId, int? trainerId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<TrainerCommissionRow>(
            StoredProcedureNames.GetTrainerCommissionReport,
            new { GymId = gymId, TrainerId = trainerId, FromDate = from, ToDate = to },
            cancellationToken);
        return rows.Select(MapCommission).ToList();
    }

    public async Task<PayrollDashboardDto> GetDashboardAsync(Guid? gymId, CancellationToken cancellationToken = default)
    {
        var row = await _sp.QuerySingleOrDefaultAsync<PayrollDashboardRow>(
            StoredProcedureNames.GetPayrollDashboard, new { GymId = gymId }, cancellationToken);
        return new PayrollDashboardDto
        {
            PayrollCostThisMonth = row?.PayrollCostThisMonth ?? 0,
            PendingSalaries = row?.PendingSalaries ?? 0,
            PaidCountThisMonth = row?.PaidCountThisMonth ?? 0
        };
    }

    public async Task<IReadOnlyList<TrendPointDto>> GetPayrollCostTrendAsync(Guid? gymId, int months, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<TrendPointRow>(
            StoredProcedureNames.GetPayrollCostTrend, new { GymId = gymId, Months = months }, cancellationToken);
        return rows.Select(r => new TrendPointDto { MonthLabel = r.MonthLabel, Value = r.PayrollCost }).ToList();
    }

    public async Task<IReadOnlyList<TrendPointDto>> GetCommissionTrendAsync(Guid? gymId, int months, CancellationToken cancellationToken = default)
    {
        var rows = await _sp.QueryAsync<TrendPointRow>(
            StoredProcedureNames.GetCommissionTrend, new { GymId = gymId, Months = months }, cancellationToken);
        return rows.Select(r => new TrendPointDto { MonthLabel = r.MonthLabel, Value = r.CommissionTotal }).ToList();
    }

    private static PayrollDto MapPayroll(PayrollRow r) => new()
    {
        Id = r.PayrollId, GymId = r.GymId, EmployeeType = r.EmployeeType, EmployeeId = r.EmployeeId,
        EmployeeUserId = r.EmployeeUserId, EmployeeName = r.EmployeeName,
        SalaryMonth = DateOnly.FromDateTime(r.SalaryMonth),
        BaseSalary = r.BaseSalary, IncentiveAmount = r.IncentiveAmount, CommissionAmount = r.CommissionAmount,
        DeductionAmount = r.DeductionAmount, NetSalary = r.NetSalary, Status = r.Status,
        PaidDate = r.PaidDate, CreatedDate = r.CreatedDate, CreatedBy = r.CreatedBy, UpdatedDate = r.UpdatedDate
    };

    private static TrainerCommissionDto MapCommission(TrainerCommissionRow r) => new()
    {
        Id = r.CommissionId, GymId = r.GymId, TrainerId = r.TrainerId, TrainerName = r.TrainerName,
        MemberId = r.MemberId, MemberName = r.MemberName, PaymentId = r.PaymentId,
        Amount = r.Amount, CreatedDate = r.CreatedDate
    };
}
