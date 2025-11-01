namespace Arrivin.Client.GraphQL;

internal class HttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new();
}
