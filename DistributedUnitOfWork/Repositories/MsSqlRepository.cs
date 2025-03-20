using DistributedUnitOfWork.Abstractions;
using DistributedUnitOfWork.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedUnitOfWork.Repositories;

/// <summary>
/// Provides a repository for managing MsSqlItem entities in a MS SQL Server database.
/// </summary>
public class MsSqlRepository : IMsSqlRepository
{
    private readonly SqlConnection _connection;

    /// <inheritdoc/>
    public IUnitOfWork UnitOfWork { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlRepository"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work associated with the repository.</param>
    /// <param name="connection">The SQL connection to use for database operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when the unitOfWork or connection is null.</exception>
    public MsSqlRepository(
        [FromKeyedServices("MsSql")] IUnitOfWork unitOfWork,
        SqlConnection connection)
    {
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <inheritdoc/>
    public async Task InsertItem(MsSqlItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        using var command = _connection.CreateCommand();

        command.CommandText = "INSERT INTO [dbo].[Items] (Description) VALUES (@Description)";
        command.Parameters.AddWithValue("@Description", item.Description);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
