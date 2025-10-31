using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public class PublishDeployment<RT>(
    PushDeployment<RT> pushDeployment,
    INix<RT> nix
) where RT : struct, HasCancel<RT>
{
    public Aff<RT, DeploymentInfo> With(Installable installable) =>
        from publishInfo in nix.EvaluateDeployment(installable)
        let tuple = ResolveBuild(publishInfo)
        from _05 in tuple.Build
        from _10 in pushDeployment.With(publishInfo.Name, publishInfo.Store, tuple.PublishPath)
        let deploymentInfo = new DeploymentInfo(
            publishInfo.Store,
            publishInfo.Derivation,
            tuple.OutPath
        )
        select deploymentInfo;

    private (Aff<RT, Unit> Build, StorePath PublishPath, Option<StorePath> OutPath) ResolveBuild(PublishInfo publishInfo) =>
        publishInfo.ShouldBuild
            ? (nix.Build(publishInfo.Derivation).Map(_ => unit), publishInfo.OutPath, publishInfo.OutPath)
            : (unitAff, publishInfo.Derivation, Option<StorePath>.None);
}