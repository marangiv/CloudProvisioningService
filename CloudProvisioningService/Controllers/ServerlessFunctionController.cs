using ProvisioningFramework.Models;
using ProvisioningFramework.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CloudProvisioningService.Controllers
{
  [ApiController]
  [Route("v2/functions")]
  public class ServerlessFunctionController : ControllerBase
  {
    private readonly IServerlessFunctionService _service;

    public ServerlessFunctionController(IServerlessFunctionService service)
    {
      _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateFunction([FromBody] ServerlessFunctionRequest request)
    {
      string functionArn = await _service.CreateFunctionAsync(request);
      return Ok(new { function_arn = functionArn });
    }

    [HttpDelete("{function_name}")]
    public async Task<IActionResult> DeleteFunction(string function_name)
    {
      bool success = await _service.DeleteFunctionAsync(function_name);
      return success ? (IActionResult)Ok() : BadRequest();
    }

    [HttpPost("{function_name}/invoke")]
    public async Task<IActionResult> InvokeFunction(string function_name, [FromBody] string payload)
    {
      string logResult = await _service.InvokeFunctionAsync(function_name, payload);
      return Ok(new { log_result = logResult });
    }
  }
}
