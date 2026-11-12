namespace Domain.Dtos.Auth
{
  using System.ComponentModel.DataAnnotations;

  public class UpdateRoleDto
  {
    [Required(ErrorMessage = " UserName is required")]
    public string UserName { get; set; }
    public RoleType NewRole { get; set; }

  }

  public enum RoleType
  {
    ADMIN,
    CUSTOMER,
    RUNNER,
    USER
  }
}
