namespace BakimZamani.Domain.Entities;

using BakimZamani.Domain.Helpers;

/// <summary>
/// Base entity class with common properties for all entities.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier using NULID format.
    /// </summary>
    public string Id { get; set; } = NulidHelper.NewNulidToString();

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Soft delete timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}

