namespace Application.Services
{
  using Application.Interfaces;
  using Domain;
  using Domain.Entities;
  using System.Threading.Tasks;

  public class UserRoleInsertionService : IUserRoleInsertionService
  {
    private readonly ApplicationDbContext dbContext;

    public UserRoleInsertionService(ApplicationDbContext dbContext)
    {
      dbContext = dbContext;
    }

    public async Task InsertCustomerAsync(ApplicationUser user, Customer customerDetails)
    {
      customerDetails.UserId = user.Id;
      dbContext.Customers.Add(customerDetails);
      await dbContext.SaveChangesAsync();
    }
  }
}