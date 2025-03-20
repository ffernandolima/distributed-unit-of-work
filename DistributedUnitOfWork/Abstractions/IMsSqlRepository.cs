using DistributedUnitOfWork.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedUnitOfWork.Abstractions;


/// <summary>
/// Defines a repository interface for managing MsSqlItem entities in a MS SQL Server database.
/// </summary>
public interface IMsSqlRepository : IRepository<MsSqlItem>
{
    /// <summary>
    /// Inserts a new MsSqlItem entity into the database.
    /// </summary>
    /// <param name="item">The MsSqlItem entity to insert.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task InsertItem(MsSqlItem item, CancellationToken cancellationToken = default);
}
