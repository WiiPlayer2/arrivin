using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public class PullDeployment<RT>(
    GetDeployment<RT> getDeployment,
    INix<RT> nix
) where RT : struct, HasCancel<RT>
{
    public Aff<RT, DeploymentInfo> With(ServerUrl serverUrl, DeploymentName name, Option<StoreUrl> useStoreOption) =>
        from deploymentInfo in getDeployment.For(serverUrl, name)
            .Bind(x => x.ToEff("Deployment not found"))
        let pullPath = deploymentInfo.OutPath.IfNone(deploymentInfo.Derivation)
        from _10 in nix.CopyFrom(useStoreOption.IfNone(deploymentInfo.StoreUrl), pullPath)
        select deploymentInfo;
}