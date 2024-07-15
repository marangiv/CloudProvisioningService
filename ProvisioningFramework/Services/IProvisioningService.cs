using System.Threading.Tasks;
using ProvisioningFramework.Models;

namespace ProvisioningFramework.Services
{
  public interface IProvisioningService
  {
    Task<bool> BindServiceAsync(string instanceId, string serviceId, string region);
    Task<bool> UnbindServiceAsync(string instanceId, string serviceId);
  }
}
