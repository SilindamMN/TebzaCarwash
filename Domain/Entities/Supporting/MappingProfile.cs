namespace Domain.Entities.Supporting
{
  using AutoMapper;
  using Domain.Dtos.Bookings;
  using Domain.Dtos.Customers;
  using Domain.Dtos.Services;
  using Domain.Dtos.Washes;
  using Domain.Dtos.Washers;
  using Domain.Dtos.Vehicles;
  using Domain.Entities;

  /// <summary>
  /// Configures object-to-object mappings for the application.
  /// </summary>
  public class MappingProfile : Profile
  {
    public MappingProfile()
    {
      this.CreateMap<Washer, WasherDto>()
        .ForMember(
          dto => dto.FirstName,
          options => options.MapFrom(washer => washer.User == null ? string.Empty : washer.User.FirstName ?? string.Empty))
        .ForMember(
          dto => dto.LastName,
          options => options.MapFrom(washer => washer.User == null ? string.Empty : washer.User.LastName ?? string.Empty))
        .ReverseMap()
        .ForMember(washer => washer.User, options => options.Ignore());
      this.CreateMap<Customer, CustomerDto>()
        .ForMember(
          dto => dto.FirstName,
          options => options.MapFrom(customer => customer.User == null ? string.Empty : customer.User.FirstName ?? string.Empty))
        .ForMember(
          dto => dto.LastName,
          options => options.MapFrom(customer => customer.User == null ? string.Empty : customer.User.LastName ?? string.Empty))
        .ReverseMap()
        .ForMember(customer => customer.User, options => options.Ignore());
      this.CreateMap<Service, ServiceDto>().ReverseMap();
      this.CreateMap<Wash, WashDto>().ReverseMap();
      this.CreateMap<Booking, BookingDto>().ReverseMap();
      this.CreateMap<Vehicle, VehicleDto>().ReverseMap();
    }
  }
}