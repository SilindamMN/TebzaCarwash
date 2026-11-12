namespace Application.Interfaces
{
  using Domain.Dtos;
  using System.Collections.Generic;
  using System.Security.Claims;
  using System.Threading.Tasks;

    /// <summary>
    /// Provides operations for saving log entries.
    /// </summary>
  public interface ILogService
  {
        /// <summary>
        /// Saves a new log entry.
        /// </summary>
        /// <param name="UserName">The name of the user associated with the log entry.</param>
        /// <param name="Description">The description of the log entry.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveNewLog(string UserName, string Description);
  }
}