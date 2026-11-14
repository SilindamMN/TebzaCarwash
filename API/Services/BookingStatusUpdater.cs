namespace API.Services;

using Domain;
using Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;

public sealed class BookingStatusUpdater : BackgroundService
{
  private readonly IServiceScopeFactory scopeFactory;
  private readonly ILogger<BookingStatusUpdater> logger;

  public BookingStatusUpdater(
      IServiceScopeFactory scopeFactory,
      ILogger<BookingStatusUpdater> logger)
  {
    this.scopeFactory = scopeFactory;
    this.logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
    do
    {
      try
      {
        await CompleteElapsedBookingsAsync(stoppingToken);
      }
      catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
      {
        break;
      }
      catch (Exception exception)
      {
        this.logger.LogError(exception, "Failed to update elapsed booking statuses.");
      }
    }
    while (await timer.WaitForNextTickAsync(stoppingToken));
  }

  private async Task CompleteElapsedBookingsAsync(CancellationToken cancellationToken)
  {
    using var scope = this.scopeFactory.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var now = DateTime.UtcNow;
    var bookings = await context.Bookings
        .Include(booking => booking.Service)
        .Include(booking => booking.Wash)
        .Where(booking =>
            booking.IsActive &&
            !booking.IsDeleted &&
            (booking.Status == BookingStatus.Requested || booking.Status == BookingStatus.Confirmed) &&
            booking.ScheduledAt <= now)
        .ToListAsync(cancellationToken);

    foreach (var booking in bookings)
    {
      if (booking.Service == null ||
          booking.Service.DurationMinutes <= 0 ||
          booking.ScheduledAt.AddMinutes(booking.Service.DurationMinutes) > now)
      {
        continue;
      }

      booking.Status = BookingStatus.Completed;
      booking.UpdatedAt = now;
      if (booking.Wash != null && booking.Wash.Status != WashStatus.Cancelled)
      {
        booking.Wash.Status = WashStatus.Completed;
        booking.Wash.CompletedAt ??= now;
        booking.Wash.UpdatedAt = now;
      }
    }

    if (context.ChangeTracker.HasChanges())
    {
      await context.SaveChangesAsync(cancellationToken);
    }
  }
}
