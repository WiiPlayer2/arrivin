using HotChocolate;
using Arrivin.Domain;
using Arrivin.Server.Application;

namespace Arrivin.Server.Web;

public class Query
{
	public Task<DeploymentInfo?> GetDeployment([Service] Deployments deployments, DeploymentName name, CancellationToken cancellationToken) =>
		deployments.GetDeploymentInfo(name, cancellationToken);
}
