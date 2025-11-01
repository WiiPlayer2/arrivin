using System;
using LanguageExt;
using LanguageExt.Effects.Traits;

namespace Arrivin.Client.Cli;

public class Runner<RT>(Func<CancellationToken, (RT Runtimme, IDisposable Scope)> fn) where RT : struct, HasCancel<RT>
{
    public async Task<Fin<T>> Run<T>(Aff<RT, T> aff, CancellationToken cancellationToken = default)
    {
        var (runtime, scope) = fn(cancellationToken);
        using (scope)
        {
            return await aff.Run(runtime);
        }
    }
}
