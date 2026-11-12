namespace API.Controllers
{
  using Application.Interfaces;
  using Domain.Dtos;
  using Domain.Entities;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Provides API endpoints for accessing logs.
    /// </summary>
  [Route("api/[controller]")]
  [ApiController]
  public class LogsController : ControllerBase
  {
        /// <summary>
        /// Provides logging services for the containing type.
        /// </summary>
    private readonly ILogService logService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogsController"/> class.
        /// </summary>
        /// <param name="logService">The service used to access logs.</param>
    public LogsController(ILogService logService)
    {
      this.logService = logService;
    }
  }
}