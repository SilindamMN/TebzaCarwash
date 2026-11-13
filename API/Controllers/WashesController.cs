namespace API.Controllers
{
  using Application.Interfaces;
  using Domain.Dtos.Washes;
  using Domain.Entities;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/washes")]
  public sealed class WashesController : CrudControllerBase<Wash, WashDto>
  {
    public WashesController(IGenericService<Wash, WashDto> genericService)
        : base(genericService)
    {
    }
  }
}
