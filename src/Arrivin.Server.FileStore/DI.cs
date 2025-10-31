using System;
using Arrivin.Server.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;

namespace Arrivin.Server.FileStore;

public static class DI
{
    public static void AddFileStore<RT>(this IServiceCollection services) where RT : struct, HasCancel<RT>
    {
        services.AddSingleton<IDeploymentStore<RT>, FileStore<RT>>();
        services.AddOptions<Configuration>()
            .ValidateDataAnnotations()
            .BindConfiguration(Configuration.KEY);
    }
}
