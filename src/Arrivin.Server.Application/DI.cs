using Microsoft.Extensions.DependencyInjection;

namespace Arrivin.Server.Application;

public static class DI
{
	public static void AddApplicationServices(this IServiceCollection services) {
		services.AddTransient<Deployments>();
	}
}
