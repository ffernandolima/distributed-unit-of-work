using DistributedUnitOfWork.Abstractions;
using DistributedUnitOfWork.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedUnitOfWork.Services;

/// <summary>
/// Provides a distributed service that supports asynchronous processing and disposal.
/// </summary>
public class DistributedService : IDistributedService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMsSqlRepository _msSqlRepository;
    private readonly INpgsqlRepository _npgsqlRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work associated with the service.</param>
    /// <param name="msSqlRepository">The repository for managing MsSqlItem entities.</param>
    /// <param name="npgsqlRepository">The repository for managing NpgsqlItem entities.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the parameters are null.</exception>
    public DistributedService(
        [FromKeyedServices("Distributed")] IUnitOfWork unitOfWork,
        IMsSqlRepository msSqlRepository,
        INpgsqlRepository npgsqlRepository)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _msSqlRepository = msSqlRepository ?? throw new ArgumentNullException(nameof(msSqlRepository));
        _npgsqlRepository = npgsqlRepository ?? throw new ArgumentNullException(nameof(npgsqlRepository));
    }

    /// <inheritdoc/>
    public async Task ProcessData(CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine("Beginning distributed transaction for data insertion...");

            var dateTimeNow = DateTime.Now;

            await _unitOfWork.ExecuteInTransaction(async () =>
            {
                await _msSqlRepository.InsertItem(
                    new MsSqlItem
                    {
                        Description = $"Item {dateTimeNow:yyyy-MM-dd HH:mm:ss.fff}"
                    },
                    cancellationToken);

                await _npgsqlRepository.InsertItem(
                   new NpgsqlItem
                   {
                       Description = $"Item {dateTimeNow:yyyy-MM-dd HH:mm:ss.fff}"
                   },
                   cancellationToken);
            },
            cancellationToken: cancellationToken);

            Console.WriteLine("Data inserted and committed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred, transaction rolled back: {ex.Message}");
        }
    }

    #region IDisposable Members

    private bool _disposed;

    /// <summary>
    /// Disposes the resources used by the <see cref="DistributedService"/> class.
    /// </summary>
    /// <param name="disposing">A value indicating whether the method is being called from the Dispose method.</param>
    protected virtual async Task DisposeAsync(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                await _unitOfWork.DisposeAsync();
            }

            _disposed = true;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Members
}
