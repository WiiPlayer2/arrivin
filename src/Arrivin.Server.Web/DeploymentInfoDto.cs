using Arrivin.Domain;

namespace Arrivin.Server.Web;

public record DeploymentInfoDto(
    StorePath Derivation,
    StorePath? OutPath
);
