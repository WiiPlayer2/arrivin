using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public class GetDeployment<RT>(IApiClient<RT> apiClient) where RT : struct, HasCancel<RT>
{
    public Aff<RT, Option<DeploymentInfo>> For(DeploymentName name) =>
        apiClient.GetDeployment(name);
}
