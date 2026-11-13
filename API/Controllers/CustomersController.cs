namespace API.Controllers
{
  using Application.Interfaces;
  using Domain.Dtos.Customers;
  using Domain.Entities;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/customers")]
  public sealed class CustomersController : CrudControllerBase<Customer, CustomerDto>
  {
    public CustomersController(IGenericService<Customer, CustomerDto> genericService)
        : base(genericService)
    {
    }
  }
}
