using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public interface IApiClient<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, Option<DeploymentInfo>> GetDeployment(DeploymentName name);

    Aff<RT, Unit> SetDeployment(DeploymentName name, DeploymentInfo deploymentInfo);
}
