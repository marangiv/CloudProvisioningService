using Amazon.SecurityToken;
using Amazon.SimpleSystemsManagement;
using AWSCommon;
using Microsoft.Extensions.DependencyInjection;
using ProvisioningFramework.Services;

namespace ProvisioningAWS.Services
{
  /// <summary>
  /// Implements the IPlugin interface for the AWS plugin, providing service configuration for AWS-related services.
  /// </summary>
  public class Plugin : IPlugin
  {
    public string Name => "AWS";

    /// <summary>
    /// Configures services for the AWS plugin, adding necessary AWS service clients and custom services to the DI container.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddSingleton<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>();
      services.AddSingleton<IAmazonSecurityTokenService, AmazonSecurityTokenServiceClient>();      
      services.AddSingleton<IBindingService, AWSProvisioningService>();
      services.AddSingleton<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>();
      services.AddSingleton<IVirtualMachineService, EC2Service>();
    }
  }
}
