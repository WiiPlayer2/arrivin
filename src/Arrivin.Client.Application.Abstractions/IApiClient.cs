using Arrivin.Domain;

namespace Arrivin.Client.Application;

public interface IApiClient
{
    Task<DeploymentInfo?> GetDeployment(DeploymentName name, CancellationToken cancellationToken = default);
    
    Task SetDeployment(DeploymentName name, DeploymentInfo deploymentInfo, CancellationToken cancellationToken = default);
}
