using ProvisioningFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProvisioningFramework.Services
{
  public interface IServerlessFunctionService
  {
    Task<string> CreateFunctionAsync(ServerlessFunctionRequest request);
    Task<bool> DeleteFunctionAsync(string functionName);
    Task<string> InvokeFunctionAsync(string functionName, string payload);
  }
}
