using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Financial;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;

namespace Gym.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly IPayrollRepository _repository;
    private readonly ITrainerRepository _trainerRepository;
    private readonly IGymRepository _gymRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IFinancialReportExporter _exporter;

    public PayrollService(
        IPayrollRepository repository,
        ITrainerRepository trainerRepository,
        IGymRepository gymRepository,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IFinancialReportExporter exporter)
    {
        _repository = repository;
        _trainerRepository = trainerRepository;
        _gymRepository = gymRepository;
        _currentUser = currentUser;
        _auditService = auditService;
        _exporter = exporter;
    }

    public async Task<PayrollDto> GetByIdAsync(int id, Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        EnsureCanViewPayroll();
        var scope = ResolveGymScopeForQuery(gymId);
        return await _repository.GetByIdAsync(id, scope, cancellationToken)
            ?? throw new KeyNotFoundException("Payroll not found.");
    }

    public async Task<PagedResultDto<PayrollDto>> GetPagedAsync(PayrollSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewPayroll();
        var scope = ResolveGymScopeForQuery(query.GymId);
        return await _repository.GetPagedAsync(scope, query, cancellationToken);
    }

    public async Task<PayrollDto> UpdateAsync(int id, UpdatePayrollDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var scope = ResolveGymScopeForQuery(dto.GymId);
        var existing = await _repository.GetByIdAsync(id, scope, cancellationToken)
            ?? throw new KeyNotFoundException("Payroll not found.");
        await _repository.UpdateAsync(id, scope, dto, cancellationToken);
        var updated = (await _repository.GetByIdAsync(id, scope, cancellationToken))!;
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = scope, EntityName = AuditEntityNames.Payroll, EntityId = id.ToString(),
            ActionType = AuditActionTypes.Update, OldValue = existing, NewValue = updated
        }, cancellationToken);
        return updated;
    }

    public async Task<GeneratePayrollResultDto> GenerateAsync(GeneratePayrollDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var scope = ResolveGymIdForMutation(dto.GymId);
        var monthStart = new DateOnly(dto.SalaryMonth.Year, dto.SalaryMonth.Month, 1);
        var count = await _repository.GenerateMonthlyAsync(
            scope, monthStart, dto.DefaultTrainerBaseSalary, dto.DefaultStaffBaseSalary, _currentUser.UserId, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = scope, EntityName = AuditEntityNames.Payroll, EntityId = monthStart.ToString("yyyy-MM"),
            ActionType = AuditActionTypes.Create, NewValue = new { GeneratedCount = count, SalaryMonth = monthStart }
        }, cancellationToken);
        return new GeneratePayrollResultDto { GeneratedCount = count, SalaryMonth = monthStart };
    }

    public async Task ApproveAsync(int id, Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var scope = ResolveGymScopeForQuery(gymId);
        await _repository.ApproveAsync(id, scope, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = scope, EntityName = AuditEntityNames.Payroll, EntityId = id.ToString(),
            ActionType = AuditActionTypes.Update, NewValue = new { Status = PayrollStatuses.Approved }
        }, cancellationToken);
    }

    public async Task PayAsync(int id, Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var scope = ResolveGymScopeForQuery(gymId);
        await _repository.PayAsync(id, scope, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = scope, EntityName = AuditEntityNames.Payroll, EntityId = id.ToString(),
            ActionType = AuditActionTypes.Mark, NewValue = new { Status = PayrollStatuses.Paid }
        }, cancellationToken);
    }

    public async Task<TrainerCommissionDto> CreateCommissionAsync(CreateTrainerCommissionDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var scope = ResolveGymIdForMutation(dto.GymId);
        var id = await _repository.CreateCommissionAsync(scope, _currentUser.UserId, dto, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = scope, EntityName = AuditEntityNames.TrainerCommission, EntityId = id.ToString(),
            ActionType = AuditActionTypes.Create, NewValue = dto
        }, cancellationToken);
        var list = await _repository.GetCommissionReportAsync(scope, dto.TrainerId, null, null, cancellationToken);
        return list.First(c => c.Id == id);
    }

    public async Task<IReadOnlyList<TrainerCommissionDto>> GetCommissionsAsync(
        FinancialReportQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewPayroll();
        var scope = ResolveAnalyticsScope(query.GymId);
        return await _repository.GetCommissionReportAsync(scope, null, query.FromDate, query.ToDate, cancellationToken);
    }

    public async Task<IReadOnlyList<TrainerCommissionDto>> GetMyCommissionsAsync(
        FinancialReportQueryDto query, CancellationToken cancellationToken = default)
    {
        if (!IsTrainerOnly() && !_currentUser.HasPermission(Permissions.ViewPayroll))
            throw new UnauthorizedAccessException();

        var trainer = await _trainerRepository.GetByUserIdAsync(_currentUser.UserId!.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException("Trainer profile not found.");
        var gymId = _currentUser.RequireGymId();
        return await _repository.GetCommissionReportAsync(gymId, trainer.Id, query.FromDate, query.ToDate, cancellationToken);
    }

    public async Task<byte[]> ExportPdfAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewPayroll();
        var scope = ResolveGymScopeForQuery(query.GymId);
        var payrolls = (await _repository.GetPagedAsync(scope, new PayrollSearchQueryDto
        {
            SalaryMonth = query.SalaryMonth, PageNumber = 1, PageSize = 5000
        }, cancellationToken)).Items;
        var title = await GetGymTitleAsync(scope, "Payroll Report", cancellationToken);
        return _exporter.ExportPayrollReportPdf(payrolls, title);
    }

    public async Task<byte[]> ExportExcelAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewPayroll();
        var scope = ResolveGymScopeForQuery(query.GymId);
        var payrolls = (await _repository.GetPagedAsync(scope, new PayrollSearchQueryDto
        {
            SalaryMonth = query.SalaryMonth, PageNumber = 1, PageSize = 5000
        }, cancellationToken)).Items;
        var title = await GetGymTitleAsync(scope, "Payroll Ledger", cancellationToken);
        return _exporter.ExportPayrollLedgerExcel(payrolls, title);
    }

    private void EnsureCanViewPayroll()
    {
        if (!_currentUser.HasPermission(Permissions.ViewPayroll))
            throw new UnauthorizedAccessException();
    }

    private void EnsureCanManage()
    {
        if (!_currentUser.HasPermission(Permissions.ManagePayroll))
            throw new UnauthorizedAccessException();
    }

    private bool IsTrainerOnly() =>
        _currentUser.HasRole(RoleNames.Trainer)
        && !_currentUser.HasRole(RoleNames.GymAdmin)
        && !_currentUser.HasRole(RoleNames.SuperAdmin);

    private Guid ResolveGymIdForMutation(Guid? dtoGymId)
    {
        if (_currentUser.HasRole(RoleNames.SuperAdmin))
        {
            if (dtoGymId is null) throw new ArgumentException("GymId is required.");
            return dtoGymId.Value;
        }
        return _currentUser.RequireGymId();
    }

    private Guid ResolveGymScopeForQuery(Guid? requestedGymId) =>
        GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);

    private Guid? ResolveAnalyticsScope(Guid? requestedGymId)
    {
        if (_currentUser.HasRole(RoleNames.SuperAdmin))
            return requestedGymId;
        return GymScopeResolver.ResolveRequired(_currentUser, requestedGymId);
    }

    private async Task<string> GetGymTitleAsync(Guid gymId, string suffix, CancellationToken cancellationToken)
    {
        var gym = await _gymRepository.GetByIdAsync(gymId, cancellationToken);
        return $"{gym?.Name ?? "Gym"} — {suffix}";
    }
}
