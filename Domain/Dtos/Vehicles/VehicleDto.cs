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
    [RegularExpression(
      @"(?i)^(?:[A-Z]{3}\s+[0-9]{3}\s+[A-Z]{2}|[A-Z]{2}\s+[0-9]{2}\s+[A-Z]{2}\s+[A-Z]{2}|[A-Z]{2}\s+[0-9]{3}\s+[A-Z]{3})$",
      ErrorMessage = "Use a South African registration such as CLS 883 MP, BV 01 BB GP, or GP 123 ABC.")]
    public string Registration { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Make { get; set; }

    [StringLength(80)]
    public string? Model { get; set; }

    [StringLength(40)]
    public string? Colour { get; set; }

    public VehicleType Type { get; set; } = VehicleType.Sedan;

    [EnumDataType(typeof(VehicleSizeTier))]
    public VehicleSizeTier SizeTier { get; set; } = VehicleSizeTier.Medium;
  }
}
