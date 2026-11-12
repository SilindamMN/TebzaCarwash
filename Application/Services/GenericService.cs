namespace Application.Services
{
  using Application.Interfaces;
  using AutoMapper;
  using Domain;
  using Domain.Dtos.General;
  using Microsoft.EntityFrameworkCore;

  public class GenericService<TEntity, TDto> : IGenericService<TEntity, TDto>
        where TEntity : class
        where TDto : class
  {
    private readonly ApplicationDbContext dataContext;
    private readonly IMapper mapper;

    public GenericService(ApplicationDbContext dataContext, IMapper mapper)
    {
      this.dataContext = dataContext;
      this.mapper = mapper;
    }

    public async Task<GeneralServiceResponseDto> CreateAsync(TDto entityDto)
    {
      try
      {
        var newEntity = this.mapper.Map<TEntity>(entityDto);
        this.dataContext.Set<TEntity>().Add(newEntity);
        await this.SaveAsync();

        var createdDto = this.mapper.Map<TDto>(newEntity);
        return GeneralServiceResponseDto.CreateResponse(true, 200, "Created Successfully");
      }
      catch (Exception ex)
      {
        return GeneralServiceResponseDto.CreateResponse(false, 400, $"Failed to create entity: {ex.Message}");
      }
    }

    public async Task<IEnumerable<TDto>> GetAllAsync()
    {
      try
      {
        var entities = await this.dataContext.Set<TEntity>()
            .Where(e => EF.Property<bool>(e, "IsActive"))
            .ToListAsync();

        var dtos = this.mapper.Map<List<TDto>>(entities);
        return dtos;
      }
      catch (Exception ex)
      {
        return Enumerable.Empty<TDto>();
      }
    }

    public async Task<TDto?> GetByIdAsync(int id)
    {
      try
      {
        var entity = await this.dataContext.Set<TEntity>().FindAsync(id);
        if (entity == null)
        {
          return null;
        }

        return this.mapper.Map<TDto>(entity);
      }
      catch (Exception ex)
      {
        return null;
      }
    }

    public async Task<GeneralServiceResponseDto> UpdateAsync(TDto entityDto)
    {
      try
      {
        var existingEntity = await this.dataContext.Set<TEntity>().FindAsync((int)typeof(TEntity).GetProperty("Id")?.GetValue(entityDto));
        if (existingEntity == null)
        {
          return GeneralServiceResponseDto.CreateResponse(false, 404, "Entity not found");
        }

        this.mapper.Map(entityDto, existingEntity);
        this.dataContext.Set<TEntity>().Update(existingEntity);
        await this.SaveAsync();

        return GeneralServiceResponseDto.CreateResponse(true, 200, "Updated successfully");
      }
      catch (Exception ex)
      {
        return GeneralServiceResponseDto.CreateResponse(false, 400, $"Failed to update entity: {ex.Message}");
      }
    }

    public async Task<GeneralServiceResponseDto> DeleteAsync(int id)
    {
      try
      {
        var entity = await this.dataContext.Set<TEntity>().FindAsync(id);
        if (entity == null)
        {
          return GeneralServiceResponseDto.CreateResponse(false, 404, "Entity not found");
        }

        this.dataContext.Set<TEntity>().Remove(entity);
        await this.SaveAsync();

        return GeneralServiceResponseDto.CreateResponse(true, 200, "Deleted successfully");
      }
      catch (Exception ex)
      {
        return GeneralServiceResponseDto.CreateResponse(false, 400, $"Failed to delete entity: {ex.Message}");
      }
    }

    private async Task SaveAsync()
    {
      try
      {
        await this.dataContext.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        GeneralServiceResponseDto.CreateResponse(false, 400, ex.Message);
      }
    }
  }
}