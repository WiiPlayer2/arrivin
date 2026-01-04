using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public interface IApiClient<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, Option<DeploymentInfo>> GetDeployment(ApiUrl apiUrl, DeploymentName name);

    Aff<RT, Unit> SetDeployment(ApiUrl apiUrl, DeploymentName name, DeploymentInfo deploymentInfo);
}
