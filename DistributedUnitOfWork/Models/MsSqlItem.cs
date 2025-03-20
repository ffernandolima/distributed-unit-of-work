namespace DistributedUnitOfWork.Models;

/// <summary>
/// Represents an item stored in a MS SQL Server database.
/// </summary>
public class MsSqlItem
{
    /// <summary>
    /// Gets or sets the unique identifier for the item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the description of the item.
    /// </summary>
    public string Description { get; set; }
}
