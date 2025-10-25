using Arrivin.Domain;

namespace Arrivin.Server.Application;

public interface IDeploymentStore
{
	Task<DeploymentInfo?> GetDeploymentInfo(DeploymentName name, CancellationToken cancellationToken = default);
}
