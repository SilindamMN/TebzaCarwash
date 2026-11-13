namespace Application.Services
{
  using Application.Interfaces;
  using AutoMapper;
  using Domain;
  using Domain.Dtos.General;
  using Domain.Entities.Supporting;
  using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Provides CRUD operations for entities of type TEntity using DTOs of type TDto.
    /// </summary>
    /// <typeparam name="TEntity">The entity type managed by the service.</typeparam>
    /// <typeparam name="TDto">The data transfer object type used to represent entities.</typeparam>
  public class GenericService<TEntity, TDto> : IGenericService<TEntity, TDto>
        where TEntity : BaseEntity<int>
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
      var newEntity = this.mapper.Map<TEntity>(entityDto);
      newEntity.Id = 0;
      this.dataContext.Set<TEntity>().Add(newEntity);
      await this.dataContext.SaveChangesAsync();

      return GeneralServiceResponseDto.CreateResponse(true, 201, "Created successfully", newEntity.Id);
    }

        /// <summary>
        /// Retrieves all active entities and maps them to DTOs.
        /// </summary>
        /// <returns>The mapped DTOs, or an empty collection if no active entities are found or an error occurs.</returns>
    public async Task<IEnumerable<TDto>> GetAllAsync()
    {
      var entities = await this.dataContext.Set<TEntity>()
          .Where(entity => entity.IsActive && !entity.IsDeleted)
          .ToListAsync();

      return this.mapper.Map<List<TDto>>(entities);
    }
        /// <summary>
        /// Retrieves the DTO with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to retrieve.</param>
        /// <returns>The mapped DTO, or <c>null</c> if the entity is not found or an error occurs.</returns>
    public async Task<TDto?> GetByIdAsync(int id)
    {
      var entity = await this.dataContext.Set<TEntity>()
          .FirstOrDefaultAsync(item => item.Id == id && item.IsActive && !item.IsDeleted);

      return entity == null ? null : this.mapper.Map<TDto>(entity);
    }
/// <summary>
/// Updates an existing entity using the values in the supplied DTO.
/// </summary>
/// <param name="entityDto">The DTO containing the values to apply to the existing entity.</param>
/// <returns>A response indicating whether the update succeeded. The response includes a 404 status if the entity is not found, a
/// 200 status if the update succeeds, or a 400 status if an exception occurs.</returns>
    public async Task<GeneralServiceResponseDto> UpdateAsync(int id, TDto entityDto)
    {
      var existingEntity = await this.dataContext.Set<TEntity>()
          .FirstOrDefaultAsync(entity => entity.Id == id && entity.IsActive && !entity.IsDeleted);
      if (existingEntity == null)
      {
        return GeneralServiceResponseDto.CreateResponse(false, 404, "Entity not found");
      }

      this.mapper.Map(entityDto, existingEntity);
      existingEntity.Id = id;
      existingEntity.UpdatedAt = DateTime.UtcNow;
      await this.dataContext.SaveChangesAsync();

      return GeneralServiceResponseDto.CreateResponse(true, 200, "Updated successfully");
    }
        /// <summary>
        /// Deletes the entity with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to delete.</param>
        /// <returns>A response indicating whether the entity was deleted. The response has a 404 status if the entity is not
        /// found, a 200 status if deletion succeeds, or a 400 status if deletion fails.</returns>
    public async Task<GeneralServiceResponseDto> DeleteAsync(int id)
    {
      var entity = await this.dataContext.Set<TEntity>()
          .FirstOrDefaultAsync(item => item.Id == id && item.IsActive && !item.IsDeleted);
      if (entity == null)
      {
        return GeneralServiceResponseDto.CreateResponse(false, 404, "Entity not found");
      }

      entity.IsActive = false;
      entity.IsDeleted = true;
      entity.UpdatedAt = DateTime.UtcNow;
      await this.dataContext.SaveChangesAsync();

      return GeneralServiceResponseDto.CreateResponse(true, 200, "Deleted successfully");
    }
  }
}