using DistributedUnitOfWork.Abstractions;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace DistributedUnitOfWork.UnitOfWorks;

/// <summary>
/// Implementation of <see cref="IUnitOfWork"/> for PostgreSQL.
/// </summary>
public class NpgsqlUnitOfWork : IUnitOfWork
{
    private NpgsqlConnection _connection;
    private NpgsqlTransaction _transaction;

    /// <inheritdoc/>
    public bool InTransaction => _transaction is not null || Transaction.Current is not null;

    /// <summary>
    /// Initializes a new instance of the <see cref="NpgsqlUnitOfWork"/> class.
    /// </summary>
    /// <param name="connection">The PostgreSQL database connection.</param>
    public NpgsqlUnitOfWork(NpgsqlConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <inheritdoc/>
    public async Task BeginTransaction(
        System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException("There's already an active transaction.");
        }

        await OpenConnection(cancellationToken);

        if (Transaction.Current is not null)
        {
            // Enlist in the ambient distributed transaction
            // No local transaction is created – TransactionScope will coordinate the commit.
            _connection.EnlistTransaction(Transaction.Current);
        }
        else
        {
            _transaction = await _connection.BeginTransactionAsync(isolationLevel, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task Commit(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction is not null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await Rollback(CancellationToken.None);

            throw;
        }
        finally
        {
            await DisposeTransaction();
        }
    }

    /// <inheritdoc/>
    public async Task Rollback(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
        }
        catch
        {
            // Ignored
        }
        finally
        {
            await DisposeTransaction();
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

    private async Task OpenConnection(CancellationToken cancellationToken)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken);
        }
    }

    private async Task CloseConnection()
    {
        if (_connection.State == System.Data.ConnectionState.Open)
        {
            await _connection.CloseAsync();
        }
    }

    private async Task DisposeTransaction()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();

            _transaction = null;
        }
    }

    private async Task DisposeConnection()
    {
        if (_connection is not null)
        {
            await CloseConnection();

            await _connection.DisposeAsync();

            _connection = null;
        }
    }

    #region IDisposable Members

    private bool _disposed;

    /// <summary>
    /// Disposes the resources used by the <see cref="NpgsqlUnitOfWork"/> class.
    /// </summary>
    /// <param name="disposing">A value indicating whether the method is being called from the Dispose method.</param>
    protected virtual async Task DisposeAsync(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                await DisposeTransaction();
                await DisposeConnection();
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
