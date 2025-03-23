using DistributedUnitOfWork.Abstractions;
using DistributedUnitOfWork.Factories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace DistributedUnitOfWork.UnitOfWorks;

/// <summary>
/// Implementation of <see cref="IUnitOfWork"/> that links multiple unit of work instances.
/// </summary>
public class DistributedLinkedUnitOfWork : IUnitOfWork
{
    private readonly IUnitOfWork[] _unitsOfWork;
    private TransactionScope _transactionScope;

    /// <inheritdoc/>
    public bool InTransaction => _transactionScope is not null || _unitsOfWork.Any(uow => uow.InTransaction);

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedLinkedUnitOfWork"/> class.
    /// </summary>
    /// <param name="msSqlUnitsOfWork">The MS SQL Server unit of work instance.</param>
    /// <param name="npgsqlUnitsOfWork">The PostgreSQL unit of work instance.</param>
    public DistributedLinkedUnitOfWork(
        [FromKeyedServices("MsSql")] IUnitOfWork msSqlUnitsOfWork,
        [FromKeyedServices("Npgsql")] IUnitOfWork npgsqlUnitsOfWork)
            : this([msSqlUnitsOfWork, npgsqlUnitsOfWork])
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedLinkedUnitOfWork"/> class.
    /// </summary>
    /// <param name="unitsOfWork">The unit of work instances to link together.</param>
    internal DistributedLinkedUnitOfWork(params IUnitOfWork[] unitsOfWork)
    {
        if (unitsOfWork.Length == 0)
        {
            throw new ArgumentException("At least one unit of work must be provided.", nameof(unitsOfWork));
        }

        _unitsOfWork = unitsOfWork;
    }

    /// <summary>
    /// Creates a linked unit of work from multiple unit of work instances.
    /// </summary>
    /// <param name="unitsOfWork">The unit of work instances to link together.</param>
    /// <returns>A linked unit of work.</returns>
    public static IUnitOfWork CreateLinkedUnitOfWork(params IUnitOfWork[] unitsOfWork)
        => new DistributedLinkedUnitOfWork(unitsOfWork);

    /// <inheritdoc/>
    public async Task BeginTransaction(
        System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        _transactionScope = TransactionScopeFactory.CreateTransactionScope(
            isolationLevel: ConvertIsolationLevel(isolationLevel),
            transactionScopeAsyncFlowOption: TransactionScopeAsyncFlowOption.Enabled);

        // Begin transaction on each underlying unit of work.
        // They will detect the ambient Transaction and enlist automatically.
        foreach (var unitOfWork in _unitsOfWork)
        {
            await unitOfWork.BeginTransaction(isolationLevel, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task Commit(CancellationToken cancellationToken = default)
    {
        try
        {
            // Commit each unit of work's local operations (if any).
            foreach (var unitOfWork in _unitsOfWork)
            {
                await unitOfWork.Commit(cancellationToken);
            }

            // Mark the distributed transaction as complete.
            _transactionScope?.Complete();
        }
        catch
        {
            await Rollback(CancellationToken.None);

            throw;
        }
        finally
        {
            DisposeTransactionScope();
        }
    }

    /// <inheritdoc/>
    public async Task Rollback(CancellationToken cancellationToken = default)
    {
        try
        {
            // Rollback each unit of work's local operations.
            foreach (var unitOfWork in _unitsOfWork)
            {
                await unitOfWork.Rollback(cancellationToken);
            }
        }
        catch
        {
            // Ignored
        }
        finally
        {
            DisposeTransactionScope();
        }
    }

    /// <inheritdoc />
    public async Task ExecuteInTransaction(
        Func<Task> action,
        Action<Exception> handler = null,
        System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        await BeginTransaction(isolationLevel, cancellationToken);

        try
        {
            await action.Invoke();

            await Commit(cancellationToken);
        }
        catch (Exception ex)
        {
            handler?.Invoke(ex);

            await Rollback(CancellationToken.None);

            throw;
        }
    }

    /// <inheritdoc />
    public async Task<TResult> ExecuteInTransaction<TResult>(
        Func<Task<TResult>> action,
        Action<Exception> handler = null,
        System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        TResult result = default!;

        await ExecuteInTransaction(
            async () => { result = await action.Invoke(); },
            handler,
            isolationLevel,
            cancellationToken);

        return result;
    }

    private void DisposeTransactionScope()
    {
        _transactionScope?.Dispose();
        _transactionScope = null;
    }

    private static IsolationLevel ConvertIsolationLevel(System.Data.IsolationLevel isolationLevel)
        => isolationLevel switch
        {
            System.Data.IsolationLevel.Chaos => IsolationLevel.Chaos,
            System.Data.IsolationLevel.ReadCommitted => IsolationLevel.ReadCommitted,
            System.Data.IsolationLevel.ReadUncommitted => IsolationLevel.ReadUncommitted,
            System.Data.IsolationLevel.RepeatableRead => IsolationLevel.RepeatableRead,
            System.Data.IsolationLevel.Serializable => IsolationLevel.Serializable,
            System.Data.IsolationLevel.Unspecified => IsolationLevel.Unspecified,
            _ => IsolationLevel.ReadCommitted
        };

    #region IDisposable Members

    private bool _disposed;

    /// <summary>
    /// Disposes the resources used by the <see cref="DistributedLinkedUnitOfWork"/> class.
    /// </summary>
    /// <param name="disposing">A value indicating whether the method is being called from the Dispose method.</param>
    protected virtual async Task DisposeAsync(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                foreach (var unitOfWork in _unitsOfWork)
                {
                    await unitOfWork.DisposeAsync();
                }

                DisposeTransactionScope();
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
