using System.Text.Json;
using Arrivin.Server.Application;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.Options;

namespace Arrivin.Server.FileStore;

public class FileStore<RT>(
    IOptions<Configuration> configurationOptions
) : IDeploymentStore<RT> where RT : struct, HasCancel<RT>
{
    private readonly DirectoryInfo directory = new(configurationOptions.Value.Path);
    
    public Aff<RT, Option<DeploymentInfo>> GetDeploymentInfo(DeploymentName name) =>
        from path in Eff(() => Path.Join(directory.FullName, $"{name}.json"))
        let exists = File.Exists(path)
        from infoOption in exists
            ? (
                from content in Aff((RT rt) => File.ReadAllTextAsync(path, rt.CancellationToken).ToValue())
                from dto in Eff(() => JsonSerializer.Deserialize<DeploymentInfoDto>(content))
                from info in Map(dto)
                select Some(info)
            )
            : SuccessAff(Option<DeploymentInfo>.None)
        select infoOption;

    public Aff<RT, Unit> SetDeploymentInfo(DeploymentName name, DeploymentInfo info) =>
        from _05 in Eff(fun(directory.Create))
        from path in Eff(() => Path.Join(directory.FullName, $"{name}.json"))
        let dto = Map(info)
        from json in Eff(() => JsonSerializer.Serialize(dto))
        from _10 in Aff((RT rt) => File.WriteAllTextAsync(path, json, rt.CancellationToken).ToUnit().ToValue())
        select unit;

    private Eff<DeploymentInfo> Map(DeploymentInfoDto dto) =>
        from derivation in Optional(dto.Derivation).ToEff().Map(StorePath.From)
        let outPath = Optional(dto.OutPath).Map(StorePath.From)
        select new DeploymentInfo(derivation, outPath);

    private DeploymentInfoDto Map(DeploymentInfo info) => new(
        info.Derivation.Value,
        info.OutPath.IfNoneUnsafe(() => null)?.Value
    );
}