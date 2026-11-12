using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Enums
{
    /// <summary>
    /// Represents the lifecycle status of a booking.
    /// </summary>
    public enum BookingStatus
    {
        Requested,
        Confirmed,
        Completed,
        Cancelled
    }
}
