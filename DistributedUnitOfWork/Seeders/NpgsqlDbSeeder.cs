using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedUnitOfWork.Seeders;

/// <summary>
/// Seeds the PostgreSQL database.
/// </summary>
public class NpgsqlDbSeeder : BaseDbSeeder
{
    private const string InternalCommandText =
        @"CREATE TABLE IF NOT EXISTS items (
              id SERIAL PRIMARY KEY,
              description VARCHAR(100) NOT NULL
          );";

    protected override string CommandText => InternalCommandText;

    /// <summary>
    /// Initializes a new instance of the <see cref="NpgsqlDbSeeder"/> class.
    /// </summary>
    public NpgsqlDbSeeder(NpgsqlConnection connection)
        : base(connection)
    { }

    /// <inheritdoc/>
    public async override Task Seed(CancellationToken cancellationToken = default)
    {
        await base.Seed(cancellationToken);

        Console.WriteLine("PostgreSQL table seeded.");
    }
}
