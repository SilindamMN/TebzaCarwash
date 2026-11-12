namespace Domain.Entities
{
    using Domain.Entities.Enums;
    using Domain.Entities.Supporting;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a vehicle owned by a customer.
    /// </summary>
    public class Vehicle : BaseEntity<int>
    {
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        public string Registration { get; set; } = string.Empty;

        public string? Make { get; set; }

        public string? Model { get; set; }

        public string? Colour { get; set; }

        public VehicleType Type { get; set; } = VehicleType.Sedan;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public ICollection<Wash> Washes { get; set; } = new List<Wash>();
    }
}