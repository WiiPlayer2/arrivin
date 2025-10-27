using Arrivin.Server.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;

namespace Arrivin.Server.InMemory;

public static class DI
{
    public static void AddInMemoryServices<RT>(this IServiceCollection services) where RT : struct, HasCancel<RT>
    {
        services.AddSingleton<IDeploymentStore<RT>, InMemoryDeploymentStore<RT>>();
    }
}
