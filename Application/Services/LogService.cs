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

  public class LogService : ILogService
  {
    private readonly ApplicationDbContext applicationDbContext;

    public LogService(ApplicationDbContext applicationDbContext)
    {
      this.applicationDbContext = applicationDbContext;
    }

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