using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public class DeployDeployment<RT>(
    PullDeployment<RT> pullDeployment,
    INix<RT> nix,
    ICli<RT> cli
) where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> With(ServerUrl serverUrl, DeploymentName name) =>
        from deploymentInfo in pullDeployment.With(serverUrl, name)
        from outPath in deploymentInfo.OutPath.Match(
            v => SuccessAff(v),
            nix.Build(deploymentInfo.Derivation))
        let activationPath = StorePath.From(Path.Join(outPath.Value, "arrivin-activate"))
        from _10 in cli.Call(activationPath)
        select unit;
}
