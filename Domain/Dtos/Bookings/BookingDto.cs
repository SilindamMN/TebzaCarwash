namespace Domain.Dtos.Bookings
{
  using Domain.Dtos.General;
  using Domain.Entities.Enums;
  using System.ComponentModel.DataAnnotations;

  public class BookingDto : EntityDto
  {
    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    [Range(1, int.MaxValue)]
    public int ServiceId { get; set; }

    [Range(1, int.MaxValue)]
    public int? WasherId { get; set; }

    public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;

    public BookingStatus Status { get; set; } = BookingStatus.Requested;
  }
}
