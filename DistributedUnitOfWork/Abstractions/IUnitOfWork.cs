using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedUnitOfWork.Abstractions;

/// <summary>
/// Represents a unit of work that manages transactions.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Gets a value indicating whether a transaction is currently in progress.
    /// </summary>
    bool InTransaction { get; }

    /// <summary>
    /// Begins a new transaction with the specified isolation level.
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task BeginTransaction(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task Commit(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task Rollback(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the specified asynchronous action within a database transaction.
    /// The transaction ensures atomicity, meaning all operations within the action
    /// are committed if successful or rolled back in case of failure.
    /// </summary>
    /// <param name="action">The asynchronous operation to execute within the transaction.</param>
    /// <param name="handler">An optional action to handle exceptions that occur during the transaction.</param>
    /// <param name="isolationLevel">The isolation level of the transaction. Defaults to <see cref="IsolationLevel.ReadCommitted"/>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteInTransaction(
        Func<Task> action,
        Action<Exception> handler = null,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the specified asynchronous action within a database transaction and returns a result.
    /// The transaction ensures atomicity, meaning all operations within the action
    /// are committed if successful or rolled back in case of failure.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the action.</typeparam>
    /// <param name="action">The asynchronous operation to execute within the transaction.</param>
    /// <param name="handler">An optional action to handle exceptions that occur during the transaction.</param>
    /// <param name="isolationLevel">The isolation level of the transaction. Defaults to <see cref="IsolationLevel.ReadCommitted"/>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation that returns a result of type <typeparamref name="TResult"/>.</returns>
    Task<TResult> ExecuteInTransaction<TResult>(
        Func<Task<TResult>> action,
        Action<Exception> handler = null,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}
