using System.Data;
using Dapper;

namespace Gym.Infrastructure.StoredProcedures;

/// <summary>
/// Executes SQL Server stored procedures via Dapper. All business data access must use this executor.
/// </summary>
public interface IStoredProcedureExecutor
{
    Task<T?> QuerySingleOrDefaultAsync<T>(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> QueryAsync<T>(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> QueryAsync<T>(
        string storedProcedure,
        DynamicParameters parameters,
        CancellationToken cancellationToken = default);

    Task<int> ExecuteAsync(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task<T> ExecuteWithOutputAsync<T>(
        string storedProcedure,
        DynamicParameters parameters,
        string outputParameterName,
        CancellationToken cancellationToken = default);
}
