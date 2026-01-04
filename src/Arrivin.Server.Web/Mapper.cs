using Arrivin.Domain;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;

namespace Arrivin.Server.Web;

internal class Mapper
{
    public static DeploymentInfoDto Map(DeploymentInfo obj) => new(
        obj.Derivation,
        obj.OutPath.ValueUnsafe()
    );

    public static DeploymentInfo Map(DeploymentInfoDto dto) => new(
        dto.Derivation,
        Prelude.Optional(dto.OutPath)
    );
}
