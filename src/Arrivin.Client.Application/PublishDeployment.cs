using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public class PublishDeployment<RT>(
    PushDeployment<RT> pushDeployment,
    SetDeployment<RT> setDeployment,
    INix<RT> nix
) where RT : struct, HasCancel<RT>
{
    public Aff<RT, DeploymentInfo> With(ServerUrl serverUrl, Installable installable, bool ignorePushErrors, NixArgs extraBuildArgs) =>
        from publishInfo in nix.EvaluateDeployment(installable, extraBuildArgs)
        let tuple = ResolveBuild(publishInfo, extraBuildArgs)
        from _05 in tuple.Build
        let deploymentInfo = new DeploymentInfo(
            publishInfo.Store,
            publishInfo.Derivation,
            tuple.OutPath
        )
        from _10 in pushDeployment.With(serverUrl, publishInfo.Name, publishInfo.Store, tuple.PublishPath)
            .Apply(x => ignorePushErrors ? x.IfFailAff(setDeployment.For(serverUrl, publishInfo.Name, deploymentInfo)) : x)
        select deploymentInfo;

    private (Aff<RT, Unit> Build, StorePath PublishPath, Option<StorePath> OutPath) ResolveBuild(PublishInfo publishInfo, NixArgs extraBuildArgs) =>
        publishInfo.ShouldBuild
            ? (nix.Build(publishInfo.Derivation, extraBuildArgs).Map(_ => unit), publishInfo.OutPath, publishInfo.OutPath)
            : (unitAff, publishInfo.Derivation, Option<StorePath>.None);
}
