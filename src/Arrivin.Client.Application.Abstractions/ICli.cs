using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Application;

public interface ICli<RT> where RT : struct, HasCancel<RT>
{
    Aff<RT, Unit> Call(StorePath command);
}
