using Arrivin.Domain;
using Arrivin.Server.Application;
using LanguageExt;
using LanguageExt.Effects.Traits;
using LanguageExt.UnsafeValueAccess;

namespace Arrivin.Server.Web;

public class Query<RT>(Runner<RT> runner) where RT : struct, HasCancel<RT>
{
    public Task<DeploymentInfo?> GetDeployment([Service] Deployments<RT> deployments, DeploymentName name, CancellationToken cancellationToken) =>
        runner.Run(deployments.GetDeploymentInfo(name), cancellationToken).Map(v => v.ValueUnsafe())!;
}
