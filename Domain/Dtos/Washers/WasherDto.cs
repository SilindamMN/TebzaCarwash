namespace Domain.Dtos.Washers
{
  using Domain.Dtos.General;
  using System.ComponentModel.DataAnnotations;

  public class WasherDto : EntityDto
  {
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime HiredOn { get; set; } = DateTime.UtcNow;
  }
}
