namespace API.Controllers
{
  using Application.Interfaces;
  using Application.Services;
  using Domain.Dtos.Bookings;
  using Domain.Entities;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/bookings")]
  public sealed class BookingsController : CrudControllerBase<Booking, BookingDto>
  {
    private readonly BookingValidationService bookingValidation;

    public BookingsController(
        IGenericService<Booking, BookingDto> genericService,
        BookingValidationService bookingValidation)
        : base(genericService)
    {
      this.bookingValidation = bookingValidation;
    }

    [HttpPost]
    public override async Task<IActionResult> Create([FromBody] BookingDto entityDto)
    {
      var validationError = await this.bookingValidation.ValidateAsync(entityDto, cancellationToken: this.HttpContext.RequestAborted);
      if (validationError != null)
      {
        return this.BadRequest(new { message = validationError });
      }

      return await base.Create(entityDto);
    }

    [HttpPut("{id:int}")]
    public override async Task<IActionResult> Update(int id, [FromBody] BookingDto entityDto)
    {
      if (entityDto.Id != 0 && entityDto.Id != id)
      {
        return this.BadRequest("The route ID must match the DTO ID.");
      }

      var validationError = await this.bookingValidation.ValidateAsync(
          entityDto,
          id,
          this.HttpContext.RequestAborted);
      if (validationError != null)
      {
        return this.BadRequest(new { message = validationError });
      }

      return await base.Update(id, entityDto);
    }
  }
}
