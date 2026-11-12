namespace Application.Services
{
  using Application.Interfaces;
  using Domain;
  using Domain.Entities;
    using Domain.Entities.Supporting;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides asynchronous insertion of customer details associated with a user.
    /// </summary>
  public class UserRoleInsertionService : IUserRoleInsertionService
  {
    private readonly ApplicationDbContext dbContext;

    public UserRoleInsertionService(ApplicationDbContext dbContext)
    {
      dbContext = dbContext;
    }

        /// <summary>
        /// Inserts customer details associated with a user.
        /// </summary>
        /// <param name="user">The user to associate with the customer details.</param>
        /// <param name="customerDetails">The customer details to insert.</param>
        /// <returns>A task that represents the asynchronous insert operation.</returns>
    public async Task InsertCustomerAsync(ApplicationUser user, Customer customerDetails)
    {
      customerDetails.UserId = user.Id;
      dbContext.Customers.Add(customerDetails);
      await dbContext.SaveChangesAsync();
    }
  }
}