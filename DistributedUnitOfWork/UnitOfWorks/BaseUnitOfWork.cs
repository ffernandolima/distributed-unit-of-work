using DistributedUnitOfWork.Abstractions;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedUnitOfWork.UnitOfWorks
{
    /// <summary>
    /// Base implementation of <see cref="IUnitOfWork"/>.
    /// </summary>
    public abstract class BaseUnitOfWork : IUnitOfWork
    {
        protected DbConnection Connection { get; private set; }
        protected DbTransaction Transaction { get; private set; }

        /// <inheritdoc/>
        public bool InTransaction => Transaction is not null || System.Transactions.Transaction.Current is not null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseUnitOfWork"/> class.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        protected BaseUnitOfWork(DbConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc/>
        public async Task BeginTransaction(
            System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default)
        {
            if (Transaction is not null)
            {
                throw new InvalidOperationException("There's already an active transaction.");
            }

            await OpenConnection(cancellationToken);

            if (System.Transactions.Transaction.Current is not null)
            {
                // Enlist in the ambient distributed transaction
                // No local transaction is created – TransactionScope will coordinate the commit.
                Connection.EnlistTransaction(System.Transactions.Transaction.Current);
            }
            else
            {
                Transaction = await Connection.BeginTransactionAsync(isolationLevel, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task Commit(CancellationToken cancellationToken = default)
        {
            try
            {
                if (Transaction is not null)
                {
                    await Transaction.CommitAsync(cancellationToken);
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
                if (Transaction is not null)
                {
                    await Transaction.RollbackAsync(cancellationToken);
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
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                await Connection.OpenAsync(cancellationToken);
            }
        }

        private async Task CloseConnection()
        {
            if (Connection.State == System.Data.ConnectionState.Open)
            {
                await Connection.CloseAsync();
            }
        }

        private async Task DisposeTransaction()
        {
            if (Transaction is not null)
            {
                await Transaction.DisposeAsync();

                Transaction = null;
            }
        }

        private async Task DisposeConnection()
        {
            if (Connection is not null)
            {
                await CloseConnection();

                await Connection.DisposeAsync();

                Connection = null;
            }
        }

        #region IDisposable Members

        private bool _disposed;

        /// <summary>
        /// Disposes the resources used by the <see cref="BaseUnitOfWork"/> class.
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
}
