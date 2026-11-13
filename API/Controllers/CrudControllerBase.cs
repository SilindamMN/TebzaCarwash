namespace API.Controllers
{
  using Application.Interfaces;
  using Domain.Dtos.General;
  using Domain.Entities.Supporting;
  using Microsoft.AspNetCore.Mvc;

  public abstract class CrudControllerBase<TEntity, TDto> : ControllerBase
      where TEntity : BaseEntity<int>
      where TDto : EntityDto
  {
    private readonly IGenericService<TEntity, TDto> genericService;

    protected CrudControllerBase(IGenericService<TEntity, TDto> genericService)
    {
      this.genericService = genericService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TDto>>> GetAll()
    {
      return this.Ok(await this.genericService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TDto>> GetById(int id)
    {
      var entity = await this.genericService.GetByIdAsync(id);
      return entity == null ? this.NotFound() : this.Ok(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TDto entityDto)
    {
      var result = await this.genericService.CreateAsync(entityDto);
      if (!result.IsSucceed)
      {
        return this.StatusCode(result.StatusCode, result);
      }

      return this.CreatedAtAction(nameof(this.GetById), new { id = result.EntityId }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TDto entityDto)
    {
      if (entityDto.Id != 0 && entityDto.Id != id)
      {
        return this.BadRequest("The route ID must match the DTO ID.");
      }

      var result = await this.genericService.UpdateAsync(id, entityDto);
      return result.IsSucceed ? this.Ok(result) : this.StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
      var result = await this.genericService.DeleteAsync(id);
      return result.IsSucceed ? this.Ok(result) : this.StatusCode(result.StatusCode, result);
    }
  }
}
