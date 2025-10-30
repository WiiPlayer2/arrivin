using System;
using System.Text.Json.Serialization;

namespace Arrivin.Client.NixCli;

internal record PublishInfoDto(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("store")] Uri? Store,
    [property: JsonPropertyName("drv")] string? Derivation,
    [property: JsonPropertyName("out")] string? OutPath,
    [property: JsonPropertyName("build")] bool? Build
);
