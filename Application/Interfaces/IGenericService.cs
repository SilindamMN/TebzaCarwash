namespace Application.Interfaces
{
  using Domain.Dtos.General;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public interface IGenericService<TEntity, TDto>
    where TEntity : class
    where TDto : class
  {
    /// <summary>
    /// Asynchronously creates a new entity from the provided DTO.
    /// </summary>
    /// <param name="entityDto">The data transfer object containing the information for the entity to be created.</param>
    Task<GeneralServiceResponseDto> CreateAsync(TDto entityDto);

    /// <summary>
    /// Asynchronously retrieves all entities as a collection of DTOs.
    /// </summary>
    /// <returns>A collection of DTOs representing the entities.</returns>
    Task<IEnumerable<TDto>> GetAllAsync();

    /// <summary>
    /// Asynchronously retrieves a single entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <returns>The DTO representing the entity if found, otherwise null.</returns>
    Task<TDto?> GetByIdAsync(int id);

    /// <summary>
    /// Asynchronously updates an existing entity with the provided DTO.
    /// </summary>
    /// <param name="entityDto">The data transfer object containing the updated information for the entity.</param>
    Task<GeneralServiceResponseDto> UpdateAsync(TDto entityDto);

    /// <summary>
    /// Asynchronously deletes an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    Task<GeneralServiceResponseDto> DeleteAsync(int id);
  }
}