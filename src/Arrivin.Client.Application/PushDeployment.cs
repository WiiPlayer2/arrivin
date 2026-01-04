using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public class PushDeployment<RT>(
    SetDeployment<RT> setDeployment,
    INix<RT> nix
) where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> With(ServerUrl serverUrl, DeploymentName name, StorePath path) =>
        from _10 in nix.CopyTo(serverUrl.Store, path)
        from derivation in nix.GetDerivation(path)
        let outPath = path == derivation ? None : Some(path)
        let deploymentInfo = new DeploymentInfo(
            derivation,
            outPath
        )
        from _20 in setDeployment.For(serverUrl, name, deploymentInfo)
        select unit;
}
