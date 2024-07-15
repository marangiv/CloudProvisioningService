namespace ProvisioningFramework.Models
{
  public class BindingRequest
  {
    public string ServiceId { get; set; }
    public string PlanId { get; set; }
    public dynamic Parameters { get; set; }
  }
}
