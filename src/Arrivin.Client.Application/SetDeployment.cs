using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public class SetDeployment<RT>(IApiClient<RT> apiClient) where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> For(DeploymentName name, DeploymentInfo deploymentInfo) =>
        apiClient.SetDeployment(name, deploymentInfo);
}
