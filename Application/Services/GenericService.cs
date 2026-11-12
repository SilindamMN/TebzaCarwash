namespace Application.Services
{
  using Application.Interfaces;
  using AutoMapper;
  using Domain;
  using Domain.Dtos.General;
  using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Provides CRUD operations for entities of type TEntity using DTOs of type TDto.
    /// </summary>
    /// <typeparam name="TEntity">The entity type managed by the service.</typeparam>
    /// <typeparam name="TDto">The data transfer object type used to represent entities.</typeparam>
  public class GenericService<TEntity, TDto> : IGenericService<TEntity, TDto>
        where TEntity : class
        where TDto : class
  {
    private readonly ApplicationDbContext dataContext;
    private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericService"/> class.
        /// </summary>
        /// <param name="dataContext">The database context used for data access.</param>
        /// <param name="mapper">The mapper used to map between objects.</param>
    public GenericService(ApplicationDbContext dataContext, IMapper mapper)
    {
      this.dataContext = dataContext;
      this.mapper = mapper;
    }
        /// <summary>
        /// Creates and saves an entity from the specified DTO.
        /// </summary>
        /// <param name="entityDto">The DTO containing the entity data.</param>
        /// <returns>A response indicating whether creation succeeded, with a status code and message.</returns>
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

        /// <summary>
        /// Retrieves all active entities and maps them to DTOs.
        /// </summary>
        /// <returns>The mapped DTOs, or an empty collection if no active entities are found or an error occurs.</returns>
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
        /// <summary>
        /// Retrieves the DTO with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to retrieve.</param>
        /// <returns>The mapped DTO, or <c>null</c> if the entity is not found or an error occurs.</returns>
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
/// <summary>
/// Updates an existing entity using the values in the supplied DTO.
/// </summary>
/// <param name="entityDto">The DTO containing the values to apply to the existing entity.</param>
/// <returns>A response indicating whether the update succeeded. The response includes a 404 status if the entity is not found, a
/// 200 status if the update succeeds, or a 400 status if an exception occurs.</returns>
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
        /// <summary>
        /// Deletes the entity with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to delete.</param>
        /// <returns>A response indicating whether the entity was deleted. The response has a 404 status if the entity is not
        /// found, a 200 status if deletion succeeds, or a 400 status if deletion fails.</returns>
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
        /// <summary>
        /// Saves pending changes to the data store.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
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