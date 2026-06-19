using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Audit;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Financial;
using Gym.Application.Interfaces;
using Gym.Domain.Constants;

namespace Gym.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _repository;
    private readonly IGymRepository _gymRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IFinancialReportExporter _exporter;

    public ExpenseService(
        IExpenseRepository repository,
        IGymRepository gymRepository,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IFinancialReportExporter exporter)
    {
        _repository = repository;
        _gymRepository = gymRepository;
        _currentUser = currentUser;
        _auditService = auditService;
        _exporter = exporter;
    }

    public async Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync(Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        var scope = ResolveGymScopeForQuery(gymId);
        var categories = await _repository.GetCategoriesAsync(scope, cancellationToken);
        if (categories.Count > 0)
            return categories;

        await _repository.SeedCategoriesAsync(scope, cancellationToken);
        return await _repository.GetCategoriesAsync(scope, cancellationToken);
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        ValidatePaymentMethod(dto.PaymentMethod);
        var gymId = ResolveGymIdForMutation(dto.GymId);
        var created = await _repository.CreateAsync(gymId, _currentUser.UserId, dto, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = gymId, EntityName = AuditEntityNames.Expense, EntityId = created.Id.ToString(),
            ActionType = AuditActionTypes.Create, NewValue = created
        }, cancellationToken);
        return created;
    }

    public async Task<ExpenseDto> UpdateAsync(int id, UpdateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        ValidatePaymentMethod(dto.PaymentMethod);
        var existing = await GetWithAccessAsync(id, dto.GymId, cancellationToken);
        await _repository.UpdateAsync(id, existing.GymId, dto, cancellationToken);
        var updated = (await _repository.GetByIdAsync(id, existing.GymId, cancellationToken))!;
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = existing.GymId, EntityName = AuditEntityNames.Expense, EntityId = id.ToString(),
            ActionType = AuditActionTypes.Update, OldValue = existing, NewValue = updated
        }, cancellationToken);
        return updated;
    }

    public async Task DeleteAsync(int id, Guid? gymId = null, CancellationToken cancellationToken = default)
    {
        EnsureCanManage();
        var existing = await GetWithAccessAsync(id, gymId, cancellationToken);
        await _repository.DeleteAsync(id, existing.GymId, cancellationToken);
        await _auditService.LogAsync(new AuditLogEntryDto
        {
            GymId = existing.GymId, EntityName = AuditEntityNames.Expense, EntityId = id.ToString(),
            ActionType = AuditActionTypes.Delete, OldValue = existing
        }, cancellationToken);
    }

    public Task<ExpenseDto> GetByIdAsync(int id, Guid? gymId = null, CancellationToken cancellationToken = default) =>
        GetWithAccessAsync(id, gymId, cancellationToken);

    public async Task<PagedResultDto<ExpenseDto>> GetPagedAsync(ExpenseSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasPermission(Permissions.ViewExpenses))
            throw new UnauthorizedAccessException();
        var scope = ResolveGymScopeForQuery(query.GymId);
        return await _repository.GetPagedAsync(scope, query, cancellationToken);
    }

    public async Task<byte[]> ExportPdfAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewAnalytics();
        var scope = ResolveGymScopeForQuery(query.GymId);
        var expenses = await _repository.SearchAllAsync(scope, new ExpenseSearchQueryDto
        {
            FromDate = query.FromDate, ToDate = query.ToDate, PageSize = 5000
        }, cancellationToken);
        var title = await GetGymTitleAsync(scope, "Expense Report", cancellationToken);
        return _exporter.ExportExpenseReportPdf(expenses, title);
    }

    public async Task<byte[]> ExportExcelAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default)
    {
        EnsureCanViewAnalytics();
        var scope = ResolveGymScopeForQuery(query.GymId);
        var expenses = await _repository.SearchAllAsync(scope, new ExpenseSearchQueryDto
        {
            FromDate = query.FromDate, ToDate = query.ToDate, PageSize = 5000
        }, cancellationToken);
        var title = await GetGymTitleAsync(scope, "Expense Ledger", cancellationToken);
        return _exporter.ExportExpenseLedgerExcel(expenses, title);
    }

    private async Task<ExpenseDto> GetWithAccessAsync(int id, Guid? gymId, CancellationToken cancellationToken)
    {
        if (!_currentUser.HasPermission(Permissions.ViewExpenses))
            throw new UnauthorizedAccessException();
        var scope = ResolveGymScopeForQuery(gymId);
        return await _repository.GetByIdAsync(id, scope, cancellationToken)
            ?? throw new KeyNotFoundException("Expense not found.");
    }

    private void EnsureCanManage()
    {
        if (!_currentUser.HasPermission(Permissions.ManageExpenses))
            throw new UnauthorizedAccessException();
    }

    private void EnsureCanViewAnalytics()
    {
        if (!_currentUser.HasPermission(Permissions.ViewFinancialAnalytics))
            throw new UnauthorizedAccessException();
    }

    private static void ValidatePaymentMethod(string method)
    {
        if (!ExpensePaymentMethods.All.Contains(method))
            throw new ArgumentException($"Invalid payment method: {method}");
    }

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

    private async Task<string> GetGymTitleAsync(Guid gymId, string suffix, CancellationToken cancellationToken)
    {
        var gym = await _gymRepository.GetByIdAsync(gymId, cancellationToken);
        return $"{gym?.Name ?? "Gym"} — {suffix}";
    }
}
