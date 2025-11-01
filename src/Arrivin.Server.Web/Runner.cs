using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Arrivin.Server.Web;

public class Runner<RT>(Func<CancellationToken, (RT Runtimme, IDisposable Scope)> fn) where RT : struct, HasCancel<RT>
{
    public async Task<T> Run<T>(Aff<RT, T> aff, CancellationToken cancellationToken = default)
    {
        var (runtime, scope) = fn(cancellationToken);
        using (scope)
        {
            var result = await aff.Run(runtime);
            return result.ThrowIfFail();
        }
    }
}
