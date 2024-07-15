using ProvisioningFramework.Models;
using ProvisioningFramework.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CloudProvisioningService.Controllers
{
  /// <summary>
  /// Handles service binding operations for service instances.
  /// </summary>
  [ApiController]
  [Route("v2/service_instances/{instance_id}/service_bindings")]
  public class ServiceBindingController : ControllerBase
  {
    // private readonly IProvisioningService _provisioningService;
    // private readonly ICredentialService _credentialService;

    private readonly IBindingService _bindingService;

    public ServiceBindingController(IBindingService bindingService)
    {
      _bindingService = bindingService;
    }

    /// <summary>
    /// Binds a service instance to a specified service binding.
    /// </summary>
    /// <param name="instance_id">The ID of the service instance to bind.</param>
    /// <param name="binding_id">The ID of the service binding.</param>
    /// <param name="region_id">The region ID where the service is to be bound.</param>
    /// <param name="request">The binding request containing necessary information for the binding operation.</param>
    /// <returns>An IActionResult indicating the result of the binding operation.</returns> 
    [HttpPut("{binding_id}")]
    public async Task<IActionResult> BindService(string instance_id, string binding_id, string region_id, [FromBody] BindingRequest request)
    {
      bool success = await _bindingService.BindServiceAsync(instance_id, binding_id, region_id);
      if (success)
      {
        (string AccessKey, string SecretKey, string SessionToken, string Region)? credentials = await _bindingService.GetBindingCredentialsAsync(binding_id);

        if (credentials != null)
        {
          return Ok(new
          {
            ServiceAccessKeyId = credentials.Value.AccessKey,
            ServiceSecretAccessKey = credentials.Value.SecretKey,
            SessionToken = credentials.Value.SessionToken,
            ServiceRegion = credentials.Value.Region
          });
        }
      }
      return BadRequest();
    }

    /// <summary>
    /// Unbinds a service instance from a specified service binding.
    /// </summary>
    /// <param name="instance_id">The ID of the service instance to unbind.</param>
    /// <param name="binding_id">The ID of the service binding to be removed.</param>
    /// <returns>An IActionResult indicating the result of the unbinding operation. Returns Ok if successful, otherwise BadRequest.</returns>
    [HttpDelete("{binding_id}")]
    public async Task<IActionResult> UnbindService(string instance_id, string binding_id)
    {
      var success = await _bindingService.UnbindServiceAsync(instance_id, binding_id);
      return success ? (IActionResult)Ok() : BadRequest();
    }
  }
}
