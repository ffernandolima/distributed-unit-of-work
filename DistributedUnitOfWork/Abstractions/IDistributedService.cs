using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedUnitOfWork.Abstractions;

/// <summary>
/// Represents a distributed service that supports asynchronous processing and disposal.
/// </summary>
public interface IDistributedService : IAsyncDisposable
{
    /// <summary>
    /// Processes data asynchronously within a distributed transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task ProcessData(CancellationToken cancellationToken = default);
}
