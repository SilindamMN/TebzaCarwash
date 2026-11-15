namespace Application.Services
{
  using Domain;
  using Domain.Dtos.Bookings;
  using Domain.Entities;
  using Domain.Entities.Enums;
  using Microsoft.EntityFrameworkCore;

  public sealed class BookingValidationService
  {
    private const int OpeningMinutes = 9 * 60;
    private const int ClosingMinutes = 21 * 60;
    private static readonly TimeSpan SouthAfricaUtcOffset = TimeSpan.FromHours(2);
    private readonly ApplicationDbContext dataContext;

    public BookingValidationService(ApplicationDbContext dataContext)
    {
      this.dataContext = dataContext;
    }

    public async Task<string?> ValidateAsync(
        BookingDto booking,
        int? editingBookingId = null,
        CancellationToken cancellationToken = default)
    {
      if (editingBookingId.HasValue)
      {
        var existingBooking = await this.dataContext.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == editingBookingId.Value && item.IsActive && !item.IsDeleted,
                cancellationToken);
        if (existingBooking?.Status == BookingStatus.Completed)
        {
          return "Completed bookings cannot be changed.";
        }
      }

      var customerExists = await this.dataContext.Customers
          .AsNoTracking()
          .AnyAsync(
              customer => customer.Id == booking.CustomerId && customer.IsActive && !customer.IsDeleted,
              cancellationToken);
      if (!customerExists)
      {
        return "Select an active customer.";
      }

      var vehicle = await this.dataContext.Vehicles
          .AsNoTracking()
          .FirstOrDefaultAsync(
              item => item.Id == booking.VehicleId && item.IsActive && !item.IsDeleted,
              cancellationToken);
      if (vehicle == null || vehicle.CustomerId != booking.CustomerId)
      {
        return "The selected vehicle does not belong to the selected customer.";
      }

      var service = await this.dataContext.Services
          .AsNoTracking()
          .FirstOrDefaultAsync(
              item => item.Id == booking.ServiceId && item.IsActive && !item.IsDeleted,
              cancellationToken);
      if (service == null || service.DurationMinutes <= 0)
      {
        return "Select an active service with a valid duration.";
      }

      if (service.PriceFor(vehicle.SizeTier) <= 0)
      {
        return "The selected service is not available for this vehicle size.";
      }

      var scheduledAtUtc = ToUtc(booking.ScheduledAt);
      if (scheduledAtUtc <= DateTime.UtcNow)
      {
        return "Bookings must be scheduled for a future date and time.";
      }

      var localStart = scheduledAtUtc + SouthAfricaUtcOffset;
      var startMinutes = localStart.Hour * 60 + localStart.Minute;
      var dayStartUtc = DateTime.SpecifyKind(localStart.Date - SouthAfricaUtcOffset, DateTimeKind.Utc);
      var dayEndUtc = dayStartUtc.AddDays(1);

      if (booking.Status != BookingStatus.Cancelled)
      {
        var sameVehicleAlreadyBooked = await this.dataContext.Bookings
            .AsNoTracking()
            .AnyAsync(
                item =>
                    item.VehicleId == booking.VehicleId &&
                    (!editingBookingId.HasValue || item.Id != editingBookingId.Value) &&
                    item.IsActive &&
                    !item.IsDeleted &&
                    item.Status != BookingStatus.Cancelled &&
                    item.ScheduledAt >= dayStartUtc &&
                    item.ScheduledAt < dayEndUtc,
                cancellationToken);
        if (sameVehicleAlreadyBooked)
        {
          return "This vehicle already has a booking on that day.";
        }
      }

      if (startMinutes < OpeningMinutes)
      {
        return "Bookings must start at or after 09:00.";
      }

      if (startMinutes + service.DurationMinutes > ClosingMinutes)
      {
        return "This service must finish by 21:00. Choose an earlier time or shorter service.";
      }

      if (booking.Status is not BookingStatus.Requested and not BookingStatus.Confirmed ||
          booking.WasherId == null)
      {
        return null;
      }

      var washerAvailable = await this.dataContext.Washers
          .AsNoTracking()
          .AnyAsync(
              washer =>
                  washer.Id == booking.WasherId &&
                  washer.IsActive &&
                  !washer.IsDeleted &&
                  washer.IsAvailable,
              cancellationToken);
      if (!washerAvailable)
      {
        return "Select an available washer.";
      }

      var endsAtUtc = scheduledAtUtc.AddMinutes(service.DurationMinutes);
      var possibleConflicts = await this.dataContext.Bookings
          .AsNoTracking()
          .Include(item => item.Service)
          .Where(item =>
              item.IsActive &&
              !item.IsDeleted &&
              item.WasherId == booking.WasherId &&
              (!editingBookingId.HasValue || item.Id != editingBookingId.Value) &&
              (item.Status == BookingStatus.Requested || item.Status == BookingStatus.Confirmed) &&
              item.ScheduledAt < endsAtUtc &&
              item.ScheduledAt > scheduledAtUtc.AddHours(-24))
          .ToListAsync(cancellationToken);

      var hasConflict = possibleConflicts.Any(item =>
          item.Service == null ||
          item.ScheduledAt.AddMinutes(item.Service.DurationMinutes) > scheduledAtUtc);
      return hasConflict ? "The selected washer already has a booking during this time." : null;
    }

    private static DateTime ToUtc(DateTime value)
    {
      return value.Kind switch
      {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
      };
    }
  }
}
