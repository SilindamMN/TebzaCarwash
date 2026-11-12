namespace Domain.Entities
{
    using Domain.Entities.Supporting;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a customer associated with an application user.
    /// </summary>
    public class Customer : BaseEntity<int>
    {
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        public ICollection<Wash> Washes { get; set; } = new List<Wash>();

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}