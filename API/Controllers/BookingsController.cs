namespace API.Controllers
{
  using Application.Interfaces;
  using Domain.Dtos.Bookings;
  using Domain.Entities;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/bookings")]
  public sealed class BookingsController : CrudControllerBase<Booking, BookingDto>
  {
    public BookingsController(IGenericService<Booking, BookingDto> genericService)
        : base(genericService)
    {
    }
  }
}
