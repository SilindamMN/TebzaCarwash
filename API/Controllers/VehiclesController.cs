namespace API.Controllers
{
  using Application.Interfaces;
  using Domain.Dtos.Vehicles;
  using Domain.Entities;
  using Microsoft.AspNetCore.Mvc;
  using System.Text.RegularExpressions;

  [ApiController]
  [Route("api/vehicles")]
  public sealed class VehiclesController : CrudControllerBase<Vehicle, VehicleDto>
  {
    public VehiclesController(IGenericService<Vehicle, VehicleDto> genericService)
        : base(genericService)
    {
    }

    [HttpPost]
    public override Task<IActionResult> Create([FromBody] VehicleDto entityDto)
    {
      entityDto.Registration = NormalizeRegistration(entityDto.Registration);
      return base.Create(entityDto);
    }

    [HttpPut("{id:int}")]
    public override Task<IActionResult> Update(int id, [FromBody] VehicleDto entityDto)
    {
      entityDto.Registration = NormalizeRegistration(entityDto.Registration);
      return base.Update(id, entityDto);
    }

    private static string NormalizeRegistration(string registration)
    {
      return Regex.Replace(registration.Trim(), @"\s+", " ").ToUpperInvariant();
    }
  }
}
