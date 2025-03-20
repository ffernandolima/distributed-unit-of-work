namespace DistributedUnitOfWork.Abstractions;

/// <summary>
/// Defines a generic repository interface for managing entities.
/// </summary>
/// <typeparam name="T">The type of entity managed by the repository.</typeparam>
public interface IRepository<T>
{
    /// <summary>
    /// Gets the unit of work associated with the repository.
    /// </summary>
    IUnitOfWork UnitOfWork { get; }
}
