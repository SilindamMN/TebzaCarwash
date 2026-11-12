namespace Domain.Entities
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations.Schema;

  public class Customer : BaseEntity<int>
  {
    [ForeignKey(nameof(ApplicationUser))]
    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
  }
}