using DistributedUnitOfWork.Abstractions;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedUnitOfWork.Seeders;

/// <summary>
/// Seeds the MS SQL Server database.
/// </summary>
public class MsSqlDbSeeder : IDbSeeder
{
    private readonly SqlConnection _connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlDbSeeder"/> class.
    /// </summary>
    public MsSqlDbSeeder(SqlConnection connection)
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
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Items')
            BEGIN
                CREATE TABLE [dbo].[Items](
                    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [Description] NVARCHAR(100) NOT NULL
                );
            END;";

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        Console.WriteLine("MS SQL Server table seeded.");
    }
}
