namespace Arrivin.Client.GraphQL;

internal class HttpClientFactory(Uri serverUrl) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new()
    {
        BaseAddress = serverUrl,
    };
}
