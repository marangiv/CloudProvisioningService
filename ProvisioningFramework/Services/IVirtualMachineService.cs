using System.Threading.Tasks;
using ProvisioningFramework.Models;

namespace ProvisioningFramework.Services
{
  public interface IVirtualMachineService
  {
    Task<string> ProvisionInstanceAsync(VirtualMachineRequest request);
    Task<bool> DeprovisionInstanceAsync(VirtualMachineRequest request);
    Task<bool> StartInstanceAsync(VirtualMachineRequest request);
    Task<bool> StopInstanceAsync(VirtualMachineRequest request);
    Task<bool> RestartInstanceAsync(VirtualMachineRequest request);
  }
}
