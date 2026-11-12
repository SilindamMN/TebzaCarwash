namespace API.Controllers
{
  using Application.Interfaces;
  using Domain.Dtos;
  using Domain.Entities;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/[controller]")]
  [ApiController]
  public class LogsController : ControllerBase
  {
    private readonly ILogService logService;

    public LogsController(ILogService logService)
    {
      this.logService = logService;
    }
  }
}