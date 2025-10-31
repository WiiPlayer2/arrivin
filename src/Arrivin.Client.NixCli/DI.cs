using System;
using Arrivin.Client.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;

namespace Arrivin.Client.NixCli;

public static class DI
{
    public static void AddNixCli<RT>(this IServiceCollection services) where RT : struct, HasCancel<RT>
    {
        services.AddSingleton<INix<RT>, NixCli<RT>>();
        services.AddTransient<ICli<RT>, Cli<RT>>();
    }
}
