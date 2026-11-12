using Domain.Entities.Supporting;

namespace Domain.Entities
{
    /// <summary>
    /// Represents a log entry with an optional user name and description.
    /// </summary>
  public class Log : BaseEntity<long>
  {
    public string? UserName { get; set; }
    public string? Description { get; set; }
  }
}
