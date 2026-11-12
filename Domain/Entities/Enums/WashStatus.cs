using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Enums
{
    /// <summary>
    /// Specifies the status of a car wash.
    /// </summary>
    public enum WashStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }
}
