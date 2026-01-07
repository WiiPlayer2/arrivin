namespace Arrivin.Client.Application;

public record PublishInfo(
    DeploymentName Name,
    StorePath Derivation,
    StorePath OutPath,
    bool ShouldBuild
);