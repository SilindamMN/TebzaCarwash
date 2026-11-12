namespace Application.Interfaces
{
  using Domain.Dtos;
  using System.Collections.Generic;
  using System.Security.Claims;
  using System.Threading.Tasks;

  public interface ILogService
  {
    Task SaveNewLog(string UserName, string Description);
  }
}