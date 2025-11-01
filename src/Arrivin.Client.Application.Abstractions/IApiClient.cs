using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public interface IApiClient<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, Option<DeploymentInfo>> GetDeployment(ServerUrl serverUrl, DeploymentName name);

    Aff<RT, Unit> SetDeployment(ServerUrl serverUrl, DeploymentName name, DeploymentInfo deploymentInfo);
}
