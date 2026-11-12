namespace Application.Interfaces
{
  using Domain.Entities;
    using Domain.Entities.Supporting;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides operations for assigning a customer role to an application user.
    /// </summary>
  public interface IUserRoleInsertionService
  {
        /// <summary>
        /// Adds customer details for the specified user.
        /// </summary>
        /// <param name="user">The user associated with the customer details.</param>
        /// <param name="customerDetails">The customer details to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
    Task InsertCustomerAsync(ApplicationUser user, Customer customerDetails);
  }
}

