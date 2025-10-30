namespace Arrivin.Client.Application;

public record PublishInfo(
    DeploymentName Name,
    StoreUrl Store,
    StorePath Derivation,
    StorePath OutPath,
    bool ShouldBuild
);