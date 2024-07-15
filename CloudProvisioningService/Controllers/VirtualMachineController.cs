using ProvisioningFramework.Models;
using ProvisioningFramework.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CloudProvisioningService.Controllers
{
  /// <summary>
  /// Controller responsible for managing virtual machine instances.
  /// </summary>
  /// <remarks>
  /// Provides endpoints for provisioning, deprovisioning, starting, stopping, and restarting virtual machine instances.
  /// </remarks>
  [ApiController]
  [Route("v2/instances")]
  public class VirtualMachineController : ControllerBase
  {
    private readonly IVirtualMachineService _service;

    public VirtualMachineController(IVirtualMachineService service)
    {
      _service = service;
    }

    /// <summary>
    /// Provisions a new virtual machine instance based on the provided request parameters.
    /// </summary>
    /// <param name="instance_id">The identifier for the instance to be provisioned. This parameter is part of the URL path.</param>
    /// <param name="request">The details of the virtual machine to be provisioned, received in the request body.</param>
    /// <returns>An IActionResult that contains the ID of the newly provisioned instance if successful;</returns>
    [HttpPut("{instance_id}")]
    public async Task<IActionResult> ProvisionInstance(string instance_id, [FromBody] VirtualMachineRequest request)
    {
      string instanceId = await _service.ProvisionInstanceAsync(request);
      return Ok(new { instance_id = instanceId });
    }

    /// <summary>
    /// Deprovisions (terminates) a virtual machine instance specified by the instance ID.
    /// </summary>
    /// <param name="instance_id">The identifier for the instance to be deprovisioned. This parameter is part of the URL path.</param>
    /// <param name="request">The details of the virtual machine to be deprovisioned, received in the request body.</param>
    /// <returns>An IActionResult indicating the result of the deprovision operation. Returns OK if successful;</returns>
    [HttpDelete("{instance_id}")]
    public async Task<IActionResult> DeprovisionInstance(string instance_id, [FromBody] VirtualMachineRequest request)
    {
      request.InstanceId = instance_id;
      bool success = await _service.DeprovisionInstanceAsync(request);
      return success ? (IActionResult)Ok() : BadRequest();
    }

    /// <summary>
    /// Starts a virtual machine instance specified by the instance ID.
    /// </summary>
    /// <param name="instance_id">The identifier for the instance to be started. This parameter is part of the URL path.</param>
    /// <param name="request">The details of the virtual machine to be started, received in the request body.</param>
    /// <returns>An IActionResult indicating the result of the start operation. Returns OK if successful; otherwise, BadRequest.</returns>
    [HttpPost("{instance_id}/start")]
    public async Task<IActionResult> StartInstance(string instance_id, [FromBody] VirtualMachineRequest request)
    {
      request.InstanceId = instance_id;
      var success = await _service.StartInstanceAsync(request);
      return success ? (IActionResult)Ok() : BadRequest();
    }

    /// <summary>
    /// Stops a virtual machine instance specified by the instance ID.
    /// </summary>
    /// <param name="instance_id">The identifier for the instance to be stopped. This parameter is part of the URL path.</param>
    /// <param name="request">The details of the virtual machine to be stopped, received in the request body.</param>
    /// <returns>An IActionResult indicating the result of the stop operation. Returns OK if successful; otherwise, BadRequest.</returns>

    [HttpPost("{instance_id}/stop")]
    public async Task<IActionResult> StopInstance(string instance_id, [FromBody] VirtualMachineRequest request)
    {
      request.InstanceId = instance_id;
      var success = await _service.StopInstanceAsync(request);
      return success ? (IActionResult)Ok() : BadRequest();
    }

    /// <summary>
    /// Restarts a virtual machine instance specified by the instance ID.
    /// </summary>
    /// <param name="instance_id">The identifier for the instance to be restarted. This parameter is part of the URL path.</param>
    /// <param name="request">The details of the virtual machine to be restarted, received in the request body.</param>
    /// <returns>An IActionResult indicating the result of the restart operation. Returns OK if successful; otherwise, BadRequest.</returns>
    [HttpPost("{instance_id}/restart")]
    public async Task<IActionResult> RestartInstance(string instance_id, [FromBody] VirtualMachineRequest request)
    {
      request.InstanceId = instance_id;
      var success = await _service.RestartInstanceAsync(request);
      return success ? (IActionResult)Ok() : BadRequest();
    }
  }
}
