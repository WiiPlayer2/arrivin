using Arrivin.Domain;
using Arrivin.Server.Application;
using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Arrivin.Server.Web;

public class Mutation<RT>(Runner<RT> runner) where RT : struct, HasCancel<RT>
{
    public Task<Unit> SetDeployment([Service] Deployments<RT> deployments, DeploymentName name, DeploymentInfo info, CancellationToken cancellationToken = default) =>
        runner.Run(deployments.SetDeploymentInfo(name, info), cancellationToken);
}
