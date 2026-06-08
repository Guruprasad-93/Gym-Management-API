using System.Data;
using Dapper;
using Gym.Application.Interfaces;
using Gym.Infrastructure.Persistence;

namespace Gym.Infrastructure.StoredProcedures;

/// <summary>
/// Dapper-based stored procedure executor. Parameterized procedures only; no ad-hoc SQL.
/// </summary>
public sealed class StoredProcedureExecutor : IStoredProcedureExecutor
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public StoredProcedureExecutor(ISqlConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public Task<T?> QuerySingleOrDefaultAsync<T>(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default) =>
        DapperContext.QuerySingleOrDefaultAsync<T>(
            _connectionFactory, storedProcedure, parameters, cancellationToken);

    public Task<IReadOnlyList<T>> QueryAsync<T>(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default) =>
        DapperContext.QueryAsync<T>(
            _connectionFactory, storedProcedure, parameters, cancellationToken);

    public Task<IReadOnlyList<T>> QueryAsync<T>(
        string storedProcedure,
        DynamicParameters parameters,
        CancellationToken cancellationToken = default) =>
        DapperContext.QueryAsync<T>(
            _connectionFactory, storedProcedure, parameters, cancellationToken);

    public Task<int> ExecuteAsync(
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default) =>
        DapperContext.ExecuteAsync(
            _connectionFactory, storedProcedure, parameters, cancellationToken);

    public Task<T> ExecuteWithOutputAsync<T>(
        string storedProcedure,
        DynamicParameters parameters,
        string outputParameterName,
        CancellationToken cancellationToken = default) =>
        DapperContext.ExecuteWithOutputAsync<T>(
            _connectionFactory, storedProcedure, parameters, outputParameterName, cancellationToken);
}
