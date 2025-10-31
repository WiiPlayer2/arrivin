using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;

namespace Arrivin.Client.Application;

public static class DI
{
    public static void AddApplicationServices<RT>(this IServiceCollection services) where RT : struct, HasCancel<RT>
    {
        services.AddTransient<GetDeployment<RT>>();
        services.AddTransient<SetDeployment<RT>>();
        services.AddTransient<PushDeployment<RT>>();
        services.AddTransient<PullDeployment<RT>>();
        services.AddTransient<PublishDeployment<RT>>();
        services.AddTransient<DeployDeployment<RT>>();
    }
}
