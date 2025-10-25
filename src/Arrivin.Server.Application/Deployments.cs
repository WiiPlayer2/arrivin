using Arrivin.Domain;

namespace Arrivin.Server.Application;

public class Deployments(IDeploymentStore store)
{
	public Task<DeploymentInfo?> GetDeploymentInfo(DeploymentName name, CancellationToken cancellationToken = default) =>
		store.GetDeploymentInfo(name, cancellationToken);
}
