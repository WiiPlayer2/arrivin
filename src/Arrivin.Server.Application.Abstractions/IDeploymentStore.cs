using LanguageExt.Effects.Traits;

namespace Arrivin.Server.Application;

public interface IDeploymentStore<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, Option<DeploymentInfo>> GetDeploymentInfo(DeploymentName name);

    Aff<RT, Unit> SetDeploymentInfo(DeploymentName name, DeploymentInfo info);
}
