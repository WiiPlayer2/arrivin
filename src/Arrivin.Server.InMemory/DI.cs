using Arrivin.Server.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Arrivin.Server.InMemory;

public static class DI
{
	public static void AddInMemoryServices(this IServiceCollection services) {
		services.AddSingleton<IDeploymentStore, InMemoryDeploymentStore>();
	}
}
