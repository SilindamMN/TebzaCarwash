namespace Application.Services
{
  using Application.Interfaces;
  using Domain;
  using Domain.Dtos;
  using Domain.Entities;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;
  using System.Security.Claims;
  using System.Threading.Tasks;
    /// <summary>
    /// Provides operations for saving log entries to the application database.
    /// </summary>
  public class LogService : ILogService
  {
        /// <summary>
        /// Provides access to the application database.
        /// </summary>
    private readonly ApplicationDbContext applicationDbContext;
        /// <summary>
        /// Initializes a new instance of the <see cref="LogService"/> class.
        /// </summary>
        /// <param name="applicationDbContext">The database context used to access application data.</param>
    public LogService(ApplicationDbContext applicationDbContext)
    {
      this.applicationDbContext = applicationDbContext;
    }
/// <summary>
/// Saves a new log entry with the specified user name and description.
/// </summary>
/// <param name="UserName">The user name associated with the log entry.</param>
/// <param name="Description">The description of the log entry.</param>
/// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveNewLog(string UserName, string Description)
    {
      var newLog = new Log()
      {
        UserName = UserName,
        Description = Description
      };

      await this.applicationDbContext.Logs.AddAsync(newLog);
      await this.applicationDbContext.SaveChangesAsync();
    }
  }
}