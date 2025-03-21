using Microsoft.Data.SqlClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedUnitOfWork.Seeders;

/// <summary>
/// Seeds the MS SQL Server database.
/// </summary>
public class MsSqlDbSeeder : BaseDbSeeder
{
    private const string InternalCommandText = 
        @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Items')
            BEGIN
                CREATE TABLE [dbo].[Items](
                    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [Description] NVARCHAR(100) NOT NULL
                );
            END;";

    protected override string CommandText => InternalCommandText;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlDbSeeder"/> class.
    /// </summary>
    public MsSqlDbSeeder(SqlConnection connection)
        : base(connection)
    { }

    /// <inheritdoc/>
    public async override Task Seed(CancellationToken cancellationToken = default)
    {
        await base.Seed(cancellationToken);

        Console.WriteLine("MS SQL Server table seeded.");
    }
}
