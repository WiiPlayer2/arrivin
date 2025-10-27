using LanguageExt.Effects.Traits;

namespace Arrivin.Server.Application;

public class Deployments<RT>(IDeploymentStore<RT> store) where RT : struct, HasCancel<RT>
{
    public Aff<RT, Option<DeploymentInfo>> GetDeploymentInfo(DeploymentName name) =>
        store.GetDeploymentInfo(name);

    public Aff<RT, Unit> SetDeploymentInfo(DeploymentName name, DeploymentInfo info) =>
        store.SetDeploymentInfo(name, info);
}
