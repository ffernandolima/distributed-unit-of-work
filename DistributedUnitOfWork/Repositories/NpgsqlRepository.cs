using DistributedUnitOfWork.Abstractions;
using DistributedUnitOfWork.Models;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedUnitOfWork.Repositories;

/// <summary>
/// Provides a repository for managing NpgsqlItem entities in a PostgreSQL database.
/// </summary>
public class NpgsqlRepository : INpgsqlRepository
{
    private readonly NpgsqlConnection _connection;

    /// <inheritdoc/>
    public IUnitOfWork UnitOfWork { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NpgsqlRepository"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work associated with the repository.</param>
    /// <param name="currentConnection">The PostgreSQL connection to use for database operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when the unitOfWork or currentConnection is null.</exception>
    public NpgsqlRepository(
        [FromKeyedServices("Npgsql")] IUnitOfWork unitOfWork,
        NpgsqlConnection currentConnection)
    {
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _connection = currentConnection ?? throw new ArgumentNullException(nameof(currentConnection));
    }

    /// <inheritdoc/>
    public async Task InsertItem(NpgsqlItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        using var command = _connection.CreateCommand();

        command.CommandText = "INSERT INTO items (description) VALUES (@description)";
        command.Parameters.AddWithValue("@Description", item.Description);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
