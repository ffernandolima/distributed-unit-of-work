using DistributedUnitOfWork.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedUnitOfWork.Abstractions;

/// <summary>
/// Defines a repository interface for managing NpgsqlItem entities in a PostgreSQL database.
/// </summary>
public interface INpgsqlRepository : IRepository<NpgsqlItem>
{
    /// <summary>
    /// Inserts a new NpgsqlItem entity into the database.
    /// </summary>
    /// <param name="item">The NpgsqlItem entity to insert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task InsertItem(NpgsqlItem item, CancellationToken cancellationToken = default);
}
