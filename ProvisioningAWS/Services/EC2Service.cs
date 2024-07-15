using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using ProvisioningFramework.Models;
using ProvisioningFramework.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProvisioningAWS.Services
{
  public class EC2Service : IVirtualMachineService
  {
    private AmazonEC2Client _ec2Client;
    private readonly IBindingService _bindingService;
    private AmazonSecurityTokenServiceClient _stsClient;

    public Func<AmazonSecurityTokenServiceClient> StsClientFactory { get; set; }
    public Func<string, string, string, string, AmazonEC2Client> Ec2ClientFactory { get; set; }


    /// <summary>
    /// Initializes a new instance of the EC2Service class.
    /// </summary>
    /// <param name="bindingService">The binding service used to retrieve AWS credentials.</param>
    /// <remarks>
    /// The constructor sets up factories for creating AmazonSecurityTokenServiceClient and AmazonEC2Client instances.
    /// These factories are used to instantiate the clients with credentials obtained from the binding service.
    /// </remarks>
    public EC2Service(IBindingService bindingService)
    {
      _bindingService = bindingService;
      StsClientFactory = () => new AmazonSecurityTokenServiceClient();
      Ec2ClientFactory = (accessKey, secretKey, sessionToken, region) =>
          new AmazonEC2Client(accessKey, secretKey, sessionToken, new AmazonEC2Config { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region) });
    }

    /// <summary>
    /// Initializes the Amazon EC2 client with credentials obtained from the binding service.
    /// </summary>
    /// <param name="bindingId">The binding identifier used to retrieve credentials.</param>
    /// <remarks>
    /// This method checks if the EC2 client is already initialized to avoid reinitialization.
    /// If not initialized, it retrieves the credentials from the binding service and uses them
    /// to create a new Amazon EC2 client instance. This client is then used for all AWS EC2 operations.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when binding credentials are not found.</exception>
    private async Task InitializeEC2ClientAsync(string bindingId)
    {

      if (_ec2Client == null)

      {
        var credentials = await _bindingService.GetBindingCredentialsAsync(bindingId);

        if (credentials == null)
        {
          throw new InvalidOperationException("Binding credentials not found.");
        }

        string accessKey = credentials.Value.AccessKey;
        string secretKey = credentials.Value.SecretKey;
        string sessionToken = credentials.Value.SessionToken;
        string region = credentials.Value.Region;

        Console.WriteLine($"AccessKey: {accessKey}, SecretKey: {secretKey}, SessionToken: {sessionToken}, Region: {region}");

        AmazonEC2Config config = new AmazonEC2Config { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region) };

        //_ec2Client = new AmazonEC2Client(accessKey, secretKey, sessionToken, config);
        //_stsClient = new AmazonSecurityTokenServiceClient(accessKey, secretKey, sessionToken);

        _ec2Client = Ec2ClientFactory(accessKey, secretKey, sessionToken, region);

        _stsClient = StsClientFactory();
      }
    }

    /// <summary>
    /// Validates the AWS Security Token Service (STS) token.
    /// </summary>
    /// <remarks>
    /// This method checks if the STS client is initialized and then makes a call to AWS STS to get the caller identity,
    /// which effectively validates the token. If the STS client is not initialized or if the token validation fails,
    /// an InvalidOperationException is thrown.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the STS client is not initialized or if the token validation fails.</exception>
    private async Task ValidateTokenAsync()
    {
      if (_stsClient == null)
      {
        throw new InvalidOperationException("STS client not initialized.");
      }

      try
      {
        var request = new GetCallerIdentityRequest();
        await _stsClient.GetCallerIdentityAsync(request);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException("STS token validation failed.", ex);
      }
    }

    /// <summary>
    /// Provisions a new EC2 instance based on the provided request parameters.
    /// </summary>
    /// <param name="request">The virtual machine request containing the details for the instance to be provisioned, including the image ID and instance type.</param>
    /// <returns>The instance ID of the newly provisioned EC2 instance.</returns>
    /// <remarks>
    /// This method initializes the EC2 client with the provided binding ID, validates the STS token,
    /// and then sends a request to AWS to run a new instance. The method returns the instance ID of the created instance.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the EC2 client initialization or STS token validation fails.</exception>
    /// <exception cref="AmazonEC2Exception">Thrown if there is an error during the instance provisioning process.</exception>
    public async Task<string> ProvisionInstanceAsync(VirtualMachineRequest request)
    {
      await InitializeEC2ClientAsync(request.BindingId);
      await ValidateTokenAsync();

      var runRequest = new RunInstancesRequest
      {
        ImageId = request.ImageId,
        InstanceType = request.InstanceType,
        MinCount = 1,
        MaxCount = 1,
      };

      RunInstancesResponse response = await _ec2Client.RunInstancesAsync(runRequest);
      return response.Reservation.Instances[0].InstanceId;
    }

    /// <summary>
    /// Terminates an EC2 instance specified by the instance ID in the request.
    /// </summary>
    /// <param name="request">The virtual machine request containing the instance ID to be terminated.</param>
    /// <returns>A boolean value indicating whether the instance was successfully terminated.</returns>
    /// <remarks>
    /// This method attempts to terminate the specified EC2 instance. If successful, it returns true.
    /// If an AmazonEC2Exception is caught, it logs the error message and returns false.
    /// </remarks>
    /// <exception cref="AmazonEC2Exception">Thrown if there is an error during the termination process.</exception>
    public async Task<bool> DeprovisionInstanceAsync(VirtualMachineRequest request)

    {
      await InitializeEC2ClientAsync(request.BindingId);
      await ValidateTokenAsync();

      try
      {
        TerminateInstancesRequest terminateRequest = new TerminateInstancesRequest
        {
          InstanceIds = new List<string> { request.InstanceId }
        };

        TerminateInstancesResponse response = await _ec2Client.TerminateInstancesAsync(terminateRequest);
        return response.TerminatingInstances.Count > 0;
      }
      catch (AmazonEC2Exception ex)
      {
        Console.WriteLine($"Error terminating instance: {ex.Message}");
        return false;
      }
    }

    /// <summary>
    /// Starts an EC2 instance specified by the instance ID in the request.
    /// </summary>
    /// <param name="request">The virtual machine request containing the instance ID to be started.</param>
    /// <returns>A boolean value indicating whether the instance was successfully started.</returns>
    /// <remarks>
    /// This method initializes the EC2 client and validates the STS token before sending a request to AWS to start the instance.
    /// It returns true if the instance is successfully started, otherwise false.
    /// </remarks>
    public async Task<bool> StartInstanceAsync(VirtualMachineRequest request)
    {
      await InitializeEC2ClientAsync(request.BindingId);
      await ValidateTokenAsync();

      StartInstancesRequest startRequest = new StartInstancesRequest
      {
        InstanceIds = new List<string> { request.InstanceId }
      };

      StartInstancesResponse response = await _ec2Client.StartInstancesAsync(startRequest);
      return response.StartingInstances.Count > 0;
    }

    /// <summary>
    /// Stops an EC2 instance specified by the instance ID in the request.
    /// </summary>
    /// <param name="request">The virtual machine request containing the instance ID to be stopped.</param>
    /// <returns>A boolean value indicating whether the instance was successfully stopped.</returns>
    /// <remarks>
    /// This method initializes the EC2 client and validates the STS token before sending a request to AWS to stop the instance.
    /// It returns true if the instance is successfully stopped, otherwise false.
    /// </remarks>
    public async Task<bool> StopInstanceAsync(VirtualMachineRequest request)
    {
      await InitializeEC2ClientAsync(request.BindingId);
      await ValidateTokenAsync();

      StopInstancesRequest stopRequest = new StopInstancesRequest
      {
        InstanceIds = new List<string> { request.InstanceId }
      };

      StopInstancesResponse response = await _ec2Client.StopInstancesAsync(stopRequest);
      return response.StoppingInstances.Count > 0;
    }

    /// <summary>
    /// Restarts an EC2 instance specified by the instance ID in the request.
    /// </summary>
    /// <param name="request">The virtual machine request containing the instance ID to be restarted.</param>
    /// <returns>A boolean value indicating whether the instance was successfully restarted.</returns>
    /// <remarks>
    /// This method performs a stop and then a start operation on the specified EC2 instance.
    /// It initializes the EC2 client and validates the STS token before sending the requests to AWS.
    /// The method returns true if the instance is successfully restarted, otherwise false.
    /// </remarks>
    public async Task<bool> RestartInstanceAsync(VirtualMachineRequest request)
    {
      await InitializeEC2ClientAsync(request.BindingId);
      await ValidateTokenAsync();

      StopInstancesRequest stopRequest = new StopInstancesRequest
      {
        InstanceIds = new List<string> { request.InstanceId }
      };

      StopInstancesResponse stopResponse = await _ec2Client.StopInstancesAsync(stopRequest);
      if (stopResponse.StoppingInstances.Count == 0)
      {
        return false;
      }

      StartInstancesRequest startRequest = new StartInstancesRequest
      {
        InstanceIds = new List<string> { request.InstanceId }
      };

      StartInstancesResponse startResponse = await _ec2Client.StartInstancesAsync(startRequest);
      return startResponse.StartingInstances.Count > 0;
    }
  }
}
