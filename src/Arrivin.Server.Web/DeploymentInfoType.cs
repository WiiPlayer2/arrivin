using Arrivin.Domain;

namespace Arrivin.Server.Web;

public class DeploymentInfoType : ObjectType<DeploymentInfoDto>
{
    protected override void Configure(IObjectTypeDescriptor<DeploymentInfoDto> descriptor)
    {
        descriptor.Name(nameof(DeploymentInfo));
    }
}
