using System.Threading;
using System.Threading.Tasks;

namespace DistributedUnitOfWork.Abstractions;

/// <summary>
/// Defines a database seeder.
/// </summary>
public interface IDbSeeder
{
    /// <summary>
    /// Seeds the database with initial data.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task Seed(CancellationToken cancellationToken = default);
}
