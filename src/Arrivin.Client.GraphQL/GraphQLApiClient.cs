using Arrivin.Client.Application;
using LanguageExt.Effects.Traits;
using LanguageExt.UnsafeValueAccess;
using StrawberryShake;

namespace Arrivin.Client.GraphQL;

internal class GraphQLApiClient<RT>(
    GetDeploymentQuery getDeploymentQuery,
    SetDeploymentMutation setDeploymentMutation
) : IApiClient<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, Option<DeploymentInfo>> GetDeployment(DeploymentName name) =>
        from result in Aff((RT rt) => getDeploymentQuery.ExecuteAsync(name.Value, rt.CancellationToken).ToValue())
        from _10 in Eff(fun(result.EnsureNoErrors))
        let deploymentInfoOption = Optional(result.Data!.Deployment)
            .Map(data => new DeploymentInfo(
                    StoreUrl.From(data.StoreUrl),
                    StorePath.From(data.Derivation),
                    Optional(data.OutPath).Map(StorePath.From).ValueUnsafe()
                )
            )
        select deploymentInfoOption;

    public Aff<RT, Unit> SetDeployment(DeploymentName name, DeploymentInfo deploymentInfo) =>
        from infoInput in SuccessEff(new DeploymentInfoDtoInput
        {
            StoreUrl = deploymentInfo.StoreUrl.Value,
            Derivation = deploymentInfo.Derivation.Value,
            OutPath = deploymentInfo.OutPath.Map(x => x.Value).ValueUnsafe(),
        })
        from result in Aff((RT rt) => setDeploymentMutation.ExecuteAsync(name.Value, infoInput, rt.CancellationToken).ToValue())
        from _10 in Eff(fun(result.EnsureNoErrors))
        select unit;
}
