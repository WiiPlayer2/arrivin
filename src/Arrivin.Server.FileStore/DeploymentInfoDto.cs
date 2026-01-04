using System;

namespace Arrivin.Server.FileStore;

internal record DeploymentInfoDto(
    string? Derivation,
    string? OutPath
);
