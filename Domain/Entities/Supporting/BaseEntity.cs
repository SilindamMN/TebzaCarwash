namespace Domain.Entities.Supporting
{
  using System;
    /// <summary>
    /// Provides a base class with an identifier, timestamps, and activity and deletion status.
    /// </summary>
    /// <remarks>New instances initialize CreatedAt and UpdatedAt to the current UTC time, IsActive to true,
    /// and IsDeleted to false.</remarks>
    /// <typeparam name="TID">The type of the entity identifier.</typeparam>
  public class BaseEntity<TID>
  {
    public TID Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
  }
}