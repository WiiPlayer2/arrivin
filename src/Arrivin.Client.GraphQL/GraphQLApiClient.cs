using Arrivin.Client.Application;
using LanguageExt.Effects.Traits;
using LanguageExt.UnsafeValueAccess;
using Microsoft.Extensions.Logging;
using StrawberryShake;

namespace Arrivin.Client.GraphQL;

internal class GraphQLApiClient<RT>(
    GetDeploymentQuery getDeploymentQuery,
    SetDeploymentMutation setDeploymentMutation,
    ILogger<GraphQLApiClient<RT>> logger
) : IApiClient<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, Option<DeploymentInfo>> GetDeployment(ServerUrl serverUrl, DeploymentName name) =>
        from _05 in Eff(fun(() => logger.LogTrace("Getting deployment \"{name}\" from \"{server}\"", name, serverUrl)))
        from result in Aff((RT rt) => getDeploymentQuery.WithRequestUri(serverUrl.Value).ExecuteAsync(name.Value, rt.CancellationToken).ToValue())
        from _10 in Eff(fun(result.EnsureNoErrors))
        let deploymentInfoOption = Optional(result.Data!.Deployment)
            .Map(data => new DeploymentInfo(
                    StoreUrl.From(data.StoreUrl),
                    StorePath.From(data.Derivation),
                    Optional(data.OutPath).Map(StorePath.From).ValueUnsafe()
                )
            )
        select deploymentInfoOption;

    public Aff<RT, Unit> SetDeployment(ServerUrl serverUrl, DeploymentName name, DeploymentInfo deploymentInfo) =>
        from _05 in Eff(fun(() => logger.LogTrace("Setting deployment \"{name}\" on \"{server}\" with {info}", name, serverUrl, deploymentInfo)))
        from infoInput in SuccessEff(new DeploymentInfoDtoInput
        {
            StoreUrl = deploymentInfo.StoreUrl.Value,
            Derivation = deploymentInfo.Derivation.Value,
            OutPath = deploymentInfo.OutPath.Map(x => x.Value).ValueUnsafe(),
        })
        from result in Aff((RT rt) => setDeploymentMutation.WithRequestUri(serverUrl.Value).ExecuteAsync(name.Value, infoInput, rt.CancellationToken).ToValue())
        from _10 in Eff(fun(result.EnsureNoErrors))
        select unit;
}
