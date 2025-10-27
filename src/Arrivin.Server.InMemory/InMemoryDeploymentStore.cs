using Arrivin.Server.Application;
using LanguageExt.Effects.Traits;

namespace Arrivin.Server.InMemory;

internal class InMemoryDeploymentStore<RT> : IDeploymentStore<RT> where RT : struct, HasCancel<RT>
{
    private readonly IDictionary<DeploymentName, DeploymentInfo> deployments = new Dictionary<DeploymentName, DeploymentInfo>();

    public Aff<RT, Option<DeploymentInfo>> GetDeploymentInfo(DeploymentName name) =>
        Eff(() => deployments.TryGetValue(name));

    public Aff<RT, Unit> SetDeploymentInfo(DeploymentName name, DeploymentInfo info) =>
        Eff(fun(() => { deployments[name] = info; }));
}
