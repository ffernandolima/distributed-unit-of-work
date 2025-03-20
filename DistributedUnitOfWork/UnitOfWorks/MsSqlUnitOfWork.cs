using DistributedUnitOfWork.Abstractions;
using Microsoft.Data.SqlClient;

namespace DistributedUnitOfWork.UnitOfWorks;

/// <summary>
/// Implementation of <see cref="IUnitOfWork"/> for MS SQL Server.
/// </summary>
public class MsSqlUnitOfWork : BaseUnitOfWork
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlUnitOfWork"/> class.
    /// </summary>
    /// <param name="connection">The MS SQL Server database connection.</param>
    public MsSqlUnitOfWork(SqlConnection connection)
        : base(connection)
    { }
}
