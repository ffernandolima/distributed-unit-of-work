using DistributedUnitOfWork.Abstractions;
using Npgsql;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedUnitOfWork.Seeders;

/// <summary>
/// Seeds the PostgreSQL database.
/// </summary>
public class NpgsqlDbSeeder : IDbSeeder
{
    private readonly NpgsqlConnection _connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="NpgsqlDbSeeder"/> class.
    /// </summary>
    public NpgsqlDbSeeder(NpgsqlConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <inheritdoc/>
    public async Task Seed(CancellationToken cancellationToken = default)
    {
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken);
        }

        var commandText = @"
            CREATE TABLE IF NOT EXISTS items (
                id SERIAL PRIMARY KEY,
                description VARCHAR(100) NOT NULL
            );";

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        Console.WriteLine("PostgreSQL table seeded.");
    }
}
