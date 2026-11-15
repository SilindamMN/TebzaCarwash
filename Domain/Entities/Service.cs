namespace Domain.Entities
{
    using Domain.Entities.Enums;
    using Domain.Entities.Supporting;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a wash service offered by the carwash, with its price and duration.
    /// </summary>
    public class Service : BaseEntity<int>
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SmallPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LargePrice { get; set; }

        public int DurationMinutes { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public ICollection<Wash> Washes { get; set; } = new List<Wash>();

        public decimal PriceFor(VehicleSizeTier sizeTier)
        {
            return sizeTier switch
            {
                VehicleSizeTier.Small => this.SmallPrice,
                VehicleSizeTier.Medium => this.Price,
                VehicleSizeTier.Large => this.LargePrice,
                _ => throw new ArgumentOutOfRangeException(nameof(sizeTier), sizeTier, null),
            };
        }
    }
}