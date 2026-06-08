using System.Data;
using Dapper;
using Gym.Application.Interfaces;

namespace Gym.Infrastructure.Persistence;

internal static class DapperContext
{
    public static async Task<T?> QuerySingleOrDefaultAsync<T>(
        ISqlConnectionFactory connectionFactory,
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<T>(
            new CommandDefinition(
                storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public static async Task<IReadOnlyList<T>> QueryAsync<T>(
        ISqlConnectionFactory connectionFactory,
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<T>(
            new CommandDefinition(
                storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.AsList();
    }

    public static async Task<IReadOnlyList<T>> QueryAsync<T>(
        ISqlConnectionFactory connectionFactory,
        string storedProcedure,
        DynamicParameters parameters,
        CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<T>(
            new CommandDefinition(
                storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.AsList();
    }

    public static async Task<int> ExecuteAsync(
        ISqlConnectionFactory connectionFactory,
        string storedProcedure,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(
            new CommandDefinition(
                storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    public static async Task<T> ExecuteWithOutputAsync<T>(
        ISqlConnectionFactory connectionFactory,
        string storedProcedure,
        DynamicParameters parameters,
        string outputParameterName,
        CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(
                storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return parameters.Get<T>(outputParameterName);
    }
}
