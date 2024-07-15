using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CloudProvisioningService.Controllers
{
  [ApiController]
  [Route("v2/catalog")]
  public class CatalogController : ControllerBase
  {
    [HttpGet]
    public IActionResult GetCatalog()
    {
      var catalog = new
      {
        services = new List<object>
                {
                    new
                    {
                        name = "compute-service",
                        id = "compute-service-id",
                        description = "Compute Service",
                        bindable = true,
                        plans = new List<object>
                        {
                            new
                            {
                                id = "basic-plan",
                                name = "basic",
                                description = "Basic Plan"
                            }
                        }
                    },
                    new
                    {
                        name = "function-service",
                        id = "function-service-id",
                        description = "Function Service",
                        bindable = true,
                        plans = new List<object>
                        {
                            new
                            {
                                id = "basic-plan",
                                name = "basic",
                                description = "Basic Plan"
                            }
                        }
                    }
                }
      };

      return Ok(catalog);
    }
  }
}
