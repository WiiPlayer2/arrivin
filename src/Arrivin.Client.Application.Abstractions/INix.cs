using LanguageExt.Common;
using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public interface INix<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, Unit> CopyTo(StoreUrl store, StorePath path);
    
    Aff<RT, Unit> CopyFrom(StoreUrl store, StorePath path);

    Aff<RT, StorePath> GetDerivation(StorePath path);
}
