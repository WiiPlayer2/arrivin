using Arrivin.Client.Application;
using Arrivin.Domain;
using StrawberryShake;

namespace Arrivin.Client.GraphQL;

internal class GraphQLApiClient(GetDeploymentQuery getDeploymentQuery) : IApiClient
{
    public async Task<DeploymentInfo?> GetDeployment(DeploymentName name, CancellationToken cancellationToken = default)
    {
        var result = await getDeploymentQuery.ExecuteAsync(name.Value, cancellationToken);
        result.EnsureNoErrors();
        if (result.Data!.Deployment is null)
            return null;

        var deploymentInfo = new DeploymentInfo(
            StoreUrl.From(result.Data.Deployment.StoreUrl),
            StorePath.From(result.Data.Deployment.Derivation),
            result.Data.Deployment.OutPath is null ? null : StorePath.From(result.Data.Deployment.OutPath)
        );
        return deploymentInfo;
    }
}