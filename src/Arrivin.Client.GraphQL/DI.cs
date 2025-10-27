using Arrivin.Client.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Arrivin.Client.GraphQL;

public static class DI
{
    public static void AddGraphQLServices(this IServiceCollection services)
    {
        services.AddGraphQLClient();
        services.AddTransient<IApiClient, GraphQLApiClient>();
    }
    
    public static void AddGraphQLServices(this IServiceCollection services, Uri serverUrl)
    {
        services.AddGraphQLServices();
        services.AddSingleton<IHttpClientFactory>(new HttpClientFactory(serverUrl));
    }
}

internal class HttpClientFactory(Uri serverUrl) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new HttpClient()
    {
        BaseAddress = serverUrl,
    };
} 