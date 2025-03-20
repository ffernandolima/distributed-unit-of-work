using DistributedUnitOfWork.Abstractions;
using Npgsql;

namespace DistributedUnitOfWork.UnitOfWorks;

/// <summary>
/// Implementation of <see cref="IUnitOfWork"/> for PostgreSQL.
/// </summary>
public class NpgsqlUnitOfWork : BaseUnitOfWork
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NpgsqlUnitOfWork"/> class.
    /// </summary>
    /// <param name="connection">The PostgreSQL database connection.</param>
    public NpgsqlUnitOfWork(NpgsqlConnection connection)
        : base(connection)
    { }
}
