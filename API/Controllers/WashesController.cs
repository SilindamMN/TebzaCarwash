namespace API.Controllers
{
  using Application.Interfaces;
  using Domain.Dtos.Washes;
  using Domain;
  using Domain.Entities;
  using Domain.Entities.Enums;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/washes")]
  public sealed class WashesController : CrudControllerBase<Wash, WashDto>
  {
    private readonly ApplicationDbContext dataContext;

    public WashesController(
        IGenericService<Wash, WashDto> genericService,
        ApplicationDbContext dataContext)
        : base(genericService)
    {
      this.dataContext = dataContext;
    }

    [HttpPost]
    public override Task<IActionResult> Create([FromBody] WashDto entityDto)
    {
      if (entityDto.Status == WashStatus.Completed && !entityDto.IsPaid)
      {
        return Task.FromResult<IActionResult>(
            this.BadRequest(new { message = "A wash must be paid before it can be completed." }));
      }

      return base.Create(entityDto);
    }

    [HttpPut("{id:int}")]
    public override async Task<IActionResult> Update(int id, [FromBody] WashDto entityDto)
    {
      if (entityDto.Id != 0 && entityDto.Id != id)
      {
        return this.BadRequest("The route ID must match the DTO ID.");
      }

      var existingWash = await this.dataContext.Washes
          .AsNoTracking()
          .FirstOrDefaultAsync(wash => wash.Id == id && wash.IsActive && !wash.IsDeleted);
      if (existingWash == null)
      {
        return await base.Update(id, entityDto);
      }

      if (existingWash.Status == WashStatus.Completed)
      {
        return this.Conflict(new { message = "Completed washes cannot be changed." });
      }

      if (entityDto.Status == WashStatus.Completed && !entityDto.IsPaid)
      {
        return this.BadRequest(new { message = "A wash must be paid before it can be completed." });
      }

      return await base.Update(id, entityDto);
    }

    [HttpDelete("{id:int}")]
    public override async Task<IActionResult> Delete(int id)
    {
      var existingWash = await this.dataContext.Washes
          .AsNoTracking()
          .FirstOrDefaultAsync(wash => wash.Id == id && wash.IsActive && !wash.IsDeleted);
      if (existingWash?.Status == WashStatus.Completed)
      {
        return this.Conflict(new { message = "Completed washes cannot be deleted." });
      }

      return await base.Delete(id);
    }
  }
}
