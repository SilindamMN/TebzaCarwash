namespace Domain.Dtos.Vehicles
{
  using Domain.Dtos.General;
  using Domain.Entities.Enums;
  using System.ComponentModel.DataAnnotations;

  public class VehicleDto : EntityDto
  {
    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    [Required]
    [StringLength(20)]
    public string Registration { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Make { get; set; }

    [StringLength(80)]
    public string? Model { get; set; }

    [StringLength(40)]
    public string? Colour { get; set; }

    public VehicleType Type { get; set; } = VehicleType.Sedan;
  }
}
