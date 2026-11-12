namespace Domain.Entities
{
    using Domain.Entities.Supporting;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a washer profile associated with an application user.
    /// </summary>
    public class Washer : BaseEntity<int>
    {
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime HiredOn { get; set; } = DateTime.UtcNow;

        public ICollection<Wash> Washes { get; set; } = new List<Wash>();

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}