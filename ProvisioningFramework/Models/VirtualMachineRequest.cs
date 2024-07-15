namespace ProvisioningFramework.Models
{
  public class VirtualMachineRequest
  {
    public string ImageId { get; set; }
    public string InstanceType { get; set; }
    public string InstanceId { get; set; }
    public string BindingId { get; set; }
  }
}
