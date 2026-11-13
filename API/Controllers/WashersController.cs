namespace API.Controllers
{
  using Application.Interfaces;
  using Domain.Dtos.Washers;
  using Domain.Entities;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/washers")]
  public sealed class WashersController : CrudControllerBase<Washer, WasherDto>
  {
    public WashersController(IGenericService<Washer, WasherDto> genericService)
        : base(genericService)
    {
    }
  }
}
