using Arrivin.Domain;

namespace Arrivin.Server.Web;

public record DeploymentInfoDto(
    StoreUrl StoreUrl,
    StorePath Derivation,
    StorePath? OutPath
);
