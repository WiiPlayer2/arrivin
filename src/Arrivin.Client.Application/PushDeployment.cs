using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public class PushDeployment<RT>(
    SetDeployment<RT> setDeployment,
    INix<RT> nix
) where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> With(ServerUrl serverUrl, DeploymentName name, StoreUrl store, StorePath path) =>
        from _10 in nix.CopyTo(store, path)
        from derivation in nix.GetDerivation(path)
        let outPath = path == derivation ? None : Some(path)
        let deploymentInfo = new DeploymentInfo(
            store,
            derivation,
            outPath
        )
        from _20 in setDeployment.For(serverUrl, name, deploymentInfo)
        select unit;
}
