using Arrivin.Domain;

namespace Arrivin.Client.Application;

public class SetDeployment(IApiClient apiClient)
{
    public Task For(DeploymentName name, DeploymentInfo deploymentInfo,
        CancellationToken cancellationToken = default) =>
        apiClient.SetDeployment(name, deploymentInfo, cancellationToken);
}