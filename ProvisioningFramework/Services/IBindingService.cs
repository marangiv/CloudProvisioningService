using System.Threading.Tasks;

namespace ProvisioningFramework.Services
{
  public interface IBindingService
  {
    Task<bool> BindServiceAsync(string instanceId, string bindingId, string region);
    Task<bool> UnbindServiceAsync(string instanceId, string bindingId);
    Task<(string AccessKey, string SecretKey, string SessionToken, string Region)?> GetBindingCredentialsAsync(string bindingId);
  }
}
