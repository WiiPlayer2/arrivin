using Arrivin.Domain;
using Arrivin.Server.Application;

namespace Arrivin.Server.InMemory;

internal class InMemoryDeploymentStore : IDeploymentStore
{
    private readonly Dictionary<DeploymentName, DeploymentInfo> deployments = new();

    public Task<DeploymentInfo?> GetDeploymentInfo(DeploymentName name, CancellationToken cancellationToken = default)
    {
        deployments.TryGetValue(name, out var deployment);
        return Task.FromResult(deployment);
    }

    public Task SetDeploymentInfo(DeploymentName name, DeploymentInfo info, CancellationToken cancellationToken = default)
    {
        deployments[name] = info;
        return Task.CompletedTask;
    }
}
