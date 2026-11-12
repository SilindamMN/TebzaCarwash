namespace Domain.Entities
{
    using Domain.Entities.Enums;
    using Domain.Entities.Supporting;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a vehicle wash, including its customer, vehicle, washer, service, price, status, and timing.
    /// </summary>
    public class Wash : BaseEntity<int>
    {
        public int? BookingId { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking? Booking { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        public int VehicleId { get; set; }

        [ForeignKey(nameof(VehicleId))]
        public Vehicle? Vehicle { get; set; }

        public int ServiceId { get; set; }

        [ForeignKey(nameof(ServiceId))]
        public Service? Service { get; set; }

        public int WasherId { get; set; }

        [ForeignKey(nameof(WasherId))]
        public Washer? Washer { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool IsPaid { get; set; }

        public WashStatus Status { get; set; } = WashStatus.Pending;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }
    }
}