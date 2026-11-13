namespace Domain.Dtos.Customers
{
  using Domain.Dtos.General;
  using System.ComponentModel.DataAnnotations;

  public class CustomerDto : EntityDto
  {
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }
  }
}
