namespace API.Controllers
{
  using Application.Interfaces;
  using Domain.Dtos.Services;
  using Domain.Entities;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/services")]
  public sealed class ServicesController : CrudControllerBase<Service, ServiceDto>
  {
    public ServicesController(IGenericService<Service, ServiceDto> genericService)
        : base(genericService)
    {
    }
  }
}
