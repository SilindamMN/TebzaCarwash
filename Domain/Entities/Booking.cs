namespace Domain.Entities
{
    using Domain.Entities.Enums;
    using Domain.Entities.Supporting;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a customer's request to have a vehicle washed at a scheduled time.
    /// </summary>
    public class Booking : BaseEntity<int>
    {
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        public int VehicleId { get; set; }

        [ForeignKey(nameof(VehicleId))]
        public Vehicle? Vehicle { get; set; }

        public int ServiceId { get; set; }

        [ForeignKey(nameof(ServiceId))]
        public Service? Service { get; set; }

        public int? WasherId { get; set; }

        [ForeignKey(nameof(WasherId))]
        public Washer? Washer { get; set; }

        public DateTime ScheduledAt { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Requested;

        public Wash? Wash { get; set; }
    }
}