namespace Application.Interfaces
{
  using Domain.Entities;
  using System.Threading.Tasks;

  public interface IUserRoleInsertionService
  {
    Task InsertCustomerAsync(ApplicationUser user, Customer customerDetails);
  }
}

