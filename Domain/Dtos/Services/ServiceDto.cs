namespace Domain.Dtos.Services
{
  using Domain.Dtos.General;
  using System.ComponentModel.DataAnnotations;

  public class ServiceDto : EntityDto
  {
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal Price { get; set; }

    [Range(1, 1440)]
    public int DurationMinutes { get; set; }
  }
}
