namespace API.Controllers
{
  using Application.Interfaces;
  using Domain.Dtos.Vehicles;
  using Domain.Entities;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/vehicles")]
  public sealed class VehiclesController : CrudControllerBase<Vehicle, VehicleDto>
  {
    public VehiclesController(IGenericService<Vehicle, VehicleDto> genericService)
        : base(genericService)
    {
    }
  }
}
