using DistributedUnitOfWork.Abstractions;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedUnitOfWork.Seeders
{
    /// <summary>
    /// Base implementation of <see cref="IDbSeeder"/>.
    /// </summary>
    public abstract class BaseDbSeeder : IDbSeeder
    {
        protected DbConnection Connection { get; }
        protected abstract string CommandText { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDbSeeder"/> class.
        /// </summary>
        protected BaseDbSeeder(DbConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc/>
        public virtual async Task Seed(CancellationToken cancellationToken = default)
        {
            if (Connection.State != ConnectionState.Open)
            {
                await Connection.OpenAsync(cancellationToken);
            }

            using var command = Connection.CreateCommand();

            command.CommandText = CommandText;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
