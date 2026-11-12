namespace Domain.Dtos.Auth
{
  using System.ComponentModel.DataAnnotations;

  public class RegisterDto
  {
    public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string UserName { get; set; }
        public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
  }
}