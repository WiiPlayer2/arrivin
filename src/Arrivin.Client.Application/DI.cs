using Microsoft.Extensions.DependencyInjection;

namespace Arrivin.Client.Application;

public static class DI
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<GetDeployment>();
        services.AddTransient<SetDeployment>();
    }
}
