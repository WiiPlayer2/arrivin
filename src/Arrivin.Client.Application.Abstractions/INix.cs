using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public interface INix<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, StorePath> Build(StorePath derivation, NixArgs extraArgs);

    Aff<RT, Unit> CopyFrom(StoreUrl store, StorePath path);

    Aff<RT, Unit> CopyTo(StoreUrl store, StorePath path);

    Aff<RT, PublishInfo> EvaluateDeployment(Installable installable, NixArgs extraArgs);

    Aff<RT, StorePath> GetDerivation(StorePath path);
}
