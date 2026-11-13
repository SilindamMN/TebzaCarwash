namespace Domain.Dtos.General
{
  public class GeneralServiceResponseDto
  {
    public bool IsSucceed { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public int? EntityId { get; set; }

    public static GeneralServiceResponseDto CreateResponse(bool isSucceed, int statusCode, string message, int? entityId = null)
    {
      return new GeneralServiceResponseDto()
      {
        IsSucceed = isSucceed,
        StatusCode = statusCode,
        Message = message,
        EntityId = entityId
      };
    }
  }
}