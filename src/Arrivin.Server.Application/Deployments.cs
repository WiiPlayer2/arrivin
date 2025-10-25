using Arrivin.Domain;

namespace Arrivin.Server.Application;

public class Deployments(IDeploymentStore store)
{
    public Task<DeploymentInfo?> GetDeploymentInfo(DeploymentName name, CancellationToken cancellationToken = default) =>
        store.GetDeploymentInfo(name, cancellationToken);

    public Task SetDeploymentInfo(DeploymentName name, DeploymentInfo info, CancellationToken cancellationToken = default) =>
        store.SetDeploymentInfo(name, info, cancellationToken);
}
