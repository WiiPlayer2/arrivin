using Arrivin.Client.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.DependencyInjection;

namespace Arrivin.Client.GraphQL;

public static class DI
{
    public static void AddGraphQLServices<RT>(this IServiceCollection services) where RT : struct, HasCancel<RT>
    {
        services.AddGraphQLClient();
        services.AddTransient<IApiClient<RT>, GraphQLApiClient<RT>>();
        services.AddSingleton<IHttpClientFactory>(new HttpClientFactory());
    }
}
