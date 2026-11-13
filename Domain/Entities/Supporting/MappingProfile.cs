namespace Domain.Entities.Supporting
{
  using AutoMapper;
  using Domain.Dtos.Bookings;
  using Domain.Dtos.Customers;
  using Domain.Dtos.Services;
  using Domain.Dtos.Washes;
  using Domain.Dtos.Washers;
  using Domain.Entities;

  /// <summary>
  /// Configures object-to-object mappings for the application.
  /// </summary>
  public class MappingProfile : Profile
  {
    public MappingProfile()
    {
      this.CreateMap<Washer, WasherDto>().ReverseMap();
      this.CreateMap<Customer, CustomerDto>().ReverseMap();
      this.CreateMap<Service, ServiceDto>().ReverseMap();
      this.CreateMap<Wash, WashDto>().ReverseMap();
      this.CreateMap<Booking, BookingDto>().ReverseMap();
    }
  }
}