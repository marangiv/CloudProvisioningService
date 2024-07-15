using Microsoft.Extensions.DependencyInjection;

namespace ProvisioningFramework.Services
{
  public interface IPlugin
  {
    string Name { get; }
    void ConfigureServices(IServiceCollection services);
  }
}
