namespace Domain.Entities
{
  public static class StaticUserRoles
  {
    public const string RUNNER = "RUNNER";
    public const string ADMIN = "ADMIN";
    public const string CUSTOMER = "CUSTOMER";
    public const string USER = "USER";

    public const string RunnerAdmin = "RUNNER,ADMIN";
    public const string AdminCustomer = "ADMIN,CUSTOMER";
    public const string AdminCustomerRunner = "RUNNER,ADMIN,CUSTOMER";
    public const string AdminCustomerRunnerUser = "RUNNER,ADMIN,CUSTOMER,USER";
  }
}
