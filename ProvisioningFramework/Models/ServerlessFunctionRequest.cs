namespace ProvisioningFramework.Models
{
  public class ServerlessFunctionRequest
  {
    public string FunctionName { get; set; }
    public string Runtime { get; set; }
    public string Role { get; set; }
    public string Handler { get; set; }
    public string CodeUri { get; set; }
    public string BlobStorageBucket { get; set; }
    public string BlobStorageKey { get; set; }
  }
}
