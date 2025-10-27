using Arrivin.Domain;

namespace Arrivin.Client.Application;

public class GetDeployment(IApiClient apiClient)
{
    public Task<DeploymentInfo?> For(DeploymentName name, CancellationToken cancellationToken = default) =>
        apiClient.GetDeployment(name, cancellationToken);
}