namespace Domain.Entities
{
  using Microsoft.AspNetCore.Identity;
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations.Schema;

  public class ApplicationUser : IdentityUser<int>
  {
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [NotMapped]
    public IList<string>? Roles { get; set; }
  }
  public class ApplicationRole : IdentityRole<int>
  {
    public ApplicationRole() : base() { }
  }
}