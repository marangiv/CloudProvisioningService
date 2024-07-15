using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using ProvisioningFramework.Services;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ProvisioningAWS.Services
{
  public class AWSProvisioningService : IProvisioningService, IBindingService
  {
    private readonly IAmazonSecurityTokenService _stsClient;
    private readonly Dictionary<string, (string AccessKey, string SecretKey, string SessionToken, string Region)> _activeBindings;


    /// <summary>
    /// Initializes a new instance of the <see cref="AWSProvisioningService"/> class.
    /// </summary>
    /// <param name="stsClient">The AWS Security Token Service (STS) client used for obtaining session tokens.</param>
    public AWSProvisioningService(IAmazonSecurityTokenService stsClient)
    {
      _stsClient = stsClient;
      _activeBindings = new Dictionary<string, (string, string, string, string)>();
    }

    /// <summary>
    /// Binds an AWS service to an instance by obtaining temporary security credentials.
    /// </summary>
    /// <param name="instanceId">The ID of the instance to bind the service to.</param>
    /// <param name="bindingId">A unique identifier for the binding operation.</param>
    /// <param name="region">The AWS region where the service is to be bound.</param>
    /// <returns>A task that represents the asynchronous bind operation. The task result is true if the binding
    public async Task<bool> BindServiceAsync(string instanceId, string bindingId, string region)
    {
      GetSessionTokenRequest getSessionTokenRequest = new GetSessionTokenRequest();
      GetSessionTokenResponse getSessionTokenResponse = await _stsClient.GetSessionTokenAsync(getSessionTokenRequest);

      Credentials credentials = getSessionTokenResponse.Credentials;

      _activeBindings[bindingId] = (credentials.AccessKeyId, credentials.SecretAccessKey, credentials.SessionToken, region);

      return true;
    }

    /// <summary>
    /// Unbinds an AWS service from an instance by removing the temporary security credentials.
    /// </summary>
    /// <param name="instanceId">The ID of the instance from which the service is to be unbound.</param>
    /// <param name="bindingId">A unique identifier for the binding operation to be removed.</param>
    /// <returns>A task that represents the asynchronous unbind operation. The task result is true if the unbind
    public Task<bool> UnbindServiceAsync(string instanceId, string bindingId)
    {
      bool removed = _activeBindings.Remove(bindingId);
      Console.WriteLine($"UnbindServiceAsync: removed={removed}, bindingId={bindingId}");
      return Task.FromResult(removed);
    }

    /// <summary>
    /// Retrieves the binding credentials for a given binding ID.
    /// </summary>
    /// <param name="bindingId">The unique identifier for the binding.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the AWS credentials (A
    public async Task<(string AccessKey, string SecretKey, string SessionToken, string Region)?> GetBindingCredentialsAsync(string bindingId)
    {
      if (_activeBindings.TryGetValue(bindingId, out var credentials))
      {
        return await Task.FromResult(credentials);
      }
      return await Task.FromResult<(string, string, string, string)?>(null);
    }
  }
}
