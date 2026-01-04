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
    public Aff<RT, Option<DeploymentInfo>> GetDeployment(ApiUrl apiUrl, DeploymentName name) =>
        from _05 in Eff(fun(() => logger.LogTrace("Getting deployment \"{name}\" from \"{server}\"", name, apiUrl)))
        from result in Aff((RT rt) => getDeploymentQuery.WithRequestUri(apiUrl.Value).ExecuteAsync(name.Value, rt.CancellationToken).ToValue())
        from _10 in Eff(fun(result.EnsureNoErrors))
        let deploymentInfoOption = Optional(result.Data!.Deployment)
            .Map(data => new DeploymentInfo(
                    StorePath.From(data.Derivation),
                    Optional(data.OutPath).Map(StorePath.From).ValueUnsafe()
                )
            )
        select deploymentInfoOption;

    public Aff<RT, Unit> SetDeployment(ApiUrl apiUrl, DeploymentName name, DeploymentInfo deploymentInfo) =>
        from _05 in Eff(fun(() => logger.LogTrace("Setting deployment \"{name}\" on \"{server}\" with {info}", name, apiUrl, deploymentInfo)))
        from infoInput in SuccessEff(new DeploymentInfoDtoInput
        {
            Derivation = deploymentInfo.Derivation.Value,
            OutPath = deploymentInfo.OutPath.Map(x => x.Value).ValueUnsafe(),
        })
        from result in Aff((RT rt) => setDeploymentMutation.WithRequestUri(apiUrl.Value).ExecuteAsync(name.Value, infoInput, rt.CancellationToken).ToValue())
        from _10 in Eff(fun(result.EnsureNoErrors))
        select unit;
}
