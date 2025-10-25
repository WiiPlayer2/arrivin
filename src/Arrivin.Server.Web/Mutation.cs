using Arrivin.Domain;
using Arrivin.Server.Application;
using LanguageExt;

namespace Arrivin.Server.Web;

public class Mutation
{
    public async Task<Unit> SetDeployment([Service] Deployments deployments, DeploymentName name, DeploymentInfo info, CancellationToken cancellationToken = default)
    {
        await deployments.SetDeploymentInfo(name, info, cancellationToken);
        return default;
    }
}
