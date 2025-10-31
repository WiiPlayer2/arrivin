using System;

namespace Arrivin.Server.FileStore;

internal record DeploymentInfoDto(
    string? StoreUrl,
    string? Derivation,
    string? OutPath
);
