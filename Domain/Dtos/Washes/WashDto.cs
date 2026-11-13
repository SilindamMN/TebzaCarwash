namespace Domain.Dtos.Washes
{
  using Domain.Dtos.General;
  using Domain.Entities.Enums;
  using System.ComponentModel.DataAnnotations;

  public class WashDto : EntityDto
  {
    public int? BookingId { get; set; }

    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    [Range(1, int.MaxValue)]
    public int ServiceId { get; set; }

    [Range(1, int.MaxValue)]
    public int WasherId { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal Price { get; set; }

    public bool IsPaid { get; set; }

    public WashStatus Status { get; set; } = WashStatus.Pending;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
  }
}
