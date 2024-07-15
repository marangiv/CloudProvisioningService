using Xunit;
using Moq;
using ProvisioningFramework.Services;
using ProvisioningFramework.Models;
using System.Threading.Tasks;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using ProvisioningAWS.Services;
using System.Collections.Generic;
using System.Threading;

public class EC2ServiceTests
{
  private Mock<IBindingService> _mockBindingService;
  private Mock<AmazonEC2Client> _mockEC2Client;
  private Mock<AmazonSecurityTokenServiceClient> _mockStsClient;

  public EC2ServiceTests()
  {
    _mockBindingService = new Mock<IBindingService>();
    _mockEC2Client = new Mock<AmazonEC2Client>();
    _mockStsClient = new Mock<AmazonSecurityTokenServiceClient>();

    // Setup binding service to return dummy credentials
    _mockBindingService.Setup(s => s.GetBindingCredentialsAsync(It.IsAny<string>()))
        .Returns(Task.FromResult<(string AccessKey, string SecretKey, string SessionToken, string Region)?>((
            "dummy_access_key", "dummy_secret_key", "dummy_session_token", "eu-central-1")));

    // Setup STS client to validate token successfully
    _mockStsClient.Setup(s => s.GetCallerIdentityAsync(It.IsAny<GetCallerIdentityRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new GetCallerIdentityResponse { Account = "123456789012", Arn = "arn:aws:sts::123456789012:assumed-role/role-name/session-name", UserId = "AKIAIOSFODNN7EXAMPLE" });
  }

  private EC2Service CreateService()
  {
    var service = new EC2Service(_mockBindingService.Object)
    {
      StsClientFactory = () => _mockStsClient.Object,
      Ec2ClientFactory = (accessKey, secretKey, sessionToken, region) =>
          _mockEC2Client.Object
    };
    return service;
  }

  [Fact]
  public async Task ProvisionInstanceAsync_ShouldReturnInstanceId()
  {
    _mockEC2Client.Setup(client => client.RunInstancesAsync(It.IsAny<RunInstancesRequest>(), default))
        .ReturnsAsync(new RunInstancesResponse
        {
          Reservation = new Reservation
          {
            Instances = new List<Instance>
                {
                        new Instance { InstanceId = "i-1234567890abcdef0" }
                }
          }
        });

    var service = CreateService();
    var result = await service.ProvisionInstanceAsync(new VirtualMachineRequest
    {
      ImageId = "ami-0346fd83e3383dcb4",
      InstanceType = "t2.micro",
      BindingId = "binding-123"
    });

    Assert.Equal("i-1234567890abcdef0", result);
  }

  [Fact]
  public async Task DeprovisionInstanceAsync_ShouldReturnTrue()
  {
    string validInstanceId = "i-1234567890abcdef0";

    _mockEC2Client.Setup(client => client.TerminateInstancesAsync(
        It.Is<TerminateInstancesRequest>(req => req.InstanceIds.Contains(validInstanceId)),
        It.IsAny<CancellationToken>()))
        .ReturnsAsync(new TerminateInstancesResponse
        {
          TerminatingInstances = new List<InstanceStateChange>
            {
                    new InstanceStateChange { InstanceId = validInstanceId }
            }
        });

    var service = CreateService();
    var result = await service.DeprovisionInstanceAsync(new VirtualMachineRequest
    {
      InstanceId = validInstanceId,
      BindingId = "binding-123"
    });

    Assert.True(result);
    _mockEC2Client.Verify(client => client.TerminateInstancesAsync(
        It.Is<TerminateInstancesRequest>(req => req.InstanceIds.Contains(validInstanceId)),
        It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task StartInstanceAsync_ShouldReturnTrue()
  {
    _mockEC2Client.Setup(client => client.StartInstancesAsync(It.IsAny<StartInstancesRequest>(), default))
        .ReturnsAsync(new StartInstancesResponse
        {
          StartingInstances = new List<InstanceStateChange>
            {
                    new InstanceStateChange { InstanceId = "i-1234567890abcdef0" }
            }
        });

    var service = CreateService();
    var result = await service.StartInstanceAsync(new VirtualMachineRequest
    {
      InstanceId = "i-1234567890abcdef0",
      BindingId = "binding-123"
    });

    Assert.True(result);
  }

  [Fact]
  public async Task StopInstanceAsync_ShouldReturnTrue()
  {
    _mockEC2Client.Setup(client => client.StopInstancesAsync(It.IsAny<StopInstancesRequest>(), default))
        .ReturnsAsync(new StopInstancesResponse
        {
          StoppingInstances = new List<InstanceStateChange>
            {
                    new InstanceStateChange { InstanceId = "i-1234567890abcdef0" }
            }
        });

    var service = CreateService();
    var result = await service.StopInstanceAsync(new VirtualMachineRequest
    {
      InstanceId = "i-1234567890abcdef0",
      BindingId = "binding-123"
    });

    Assert.True(result);
  }

  [Fact]
  public async Task RestartInstanceAsync_ShouldReturnTrue()
  {
    _mockEC2Client.SetupSequence(client => client.StopInstancesAsync(It.IsAny<StopInstancesRequest>(), default))
        .ReturnsAsync(new StopInstancesResponse
        {
          StoppingInstances = new List<InstanceStateChange>
            {
                    new InstanceStateChange { InstanceId = "i-1234567890abcdef0" }
            }
        });

    _mockEC2Client.SetupSequence(client => client.StartInstancesAsync(It.IsAny<StartInstancesRequest>(), default))
        .ReturnsAsync(new StartInstancesResponse
        {
          StartingInstances = new List<InstanceStateChange>
            {
                    new InstanceStateChange { InstanceId = "i-1234567890abcdef0" }
            }
        });

    var service = CreateService();
    var result = await service.RestartInstanceAsync(new VirtualMachineRequest
    {
      InstanceId = "i-1234567890abcdef0",
      BindingId = "binding-123"
    });

    Assert.True(result);
  }
}
