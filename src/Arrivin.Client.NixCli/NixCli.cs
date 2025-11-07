using System.Text.Json;
using Arrivin.Client.Application;
using Arrivin.Client.Domain;
using Arrivin.Domain;
using CliWrap;
using CliWrap.Buffered;
using LanguageExt;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;

namespace Arrivin.Client.NixCli;

public class NixCli<RT>(ILogger<NixCli<RT>> logger) : INix<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, StorePath> Build(StorePath derivation, NixArgs extraArgs) =>
        from _10 in Eff(fun(() => logger.LogTrace("Building \"{derivation}\" using extra args: {extraArgs}", derivation, extraArgs)))
        from result in CliNix("nix-store", [
            "--realise",
            derivation.Value,
            ..extraArgs.Value,
        ])
        let outPath = StorePath.From(result.StandardOutput.Split('\n').First().Trim())
        select outPath;

    public Aff<RT, Unit> CopyFrom(StoreUrl store, StorePath path) =>
        from _10 in Eff(fun(() => logger.LogTrace("Copying \"{path}\" from \"{store}\"", path, store)))
        from _20 in Aff(async (RT rt) => await Cli.Wrap("nix")
            .WithArguments([
                "copy",
                "--from",
                store.Value.ToString().TrimEnd('/'),
                path.Value,
                "--no-check-sigs",
            ])
            .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
            .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
            .ExecuteAsync(rt.CancellationToken)
        ).Map(_ => unit)
        select unit;

    public Aff<RT, Unit> CopyTo(StoreUrl store, StorePath path) =>
        from _10 in Eff(fun(() => logger.LogTrace("Copying \"{path}\" to \"{store}\"", path, store)))
        from _20 in Aff(async (RT rt) => await Cli.Wrap("nix")
            .WithArguments([
                "copy",
                "--to",
                store.Value.ToString().TrimEnd('/'),
                path.Value,
                "--no-check-sigs",
            ])
            .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
            .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
            .ExecuteAsync(rt.CancellationToken)
        ).Map(_ => unit)
        select unit;

    public Aff<RT, PublishInfo> EvaluateDeployment(Installable installable, NixArgs extraArgs) =>
        from _10 in Eff(fun(() => logger.LogTrace("Evaluating \"{installable}\" with extra args: {extraArgs}", installable, extraArgs)))
        from result in CliNix("nix", ["eval", "--json", installable.Value, ..extraArgs.Value])
        from dto in Eff(() => JsonSerializer.Deserialize<PublishInfoDto>(result.StandardOutput))
        from publishInfo in (
            from name in Optional(dto.Name).ToEff().Map(DeploymentName.From)
            from store in Optional(dto.Store).ToEff().Map(StoreUrl.From)
            from derivation in Optional(dto.Derivation).ToEff().Map(StorePath.From)
            from outPath in Optional(dto.OutPath).ToEff().Map(StorePath.From)
            from build in Optional(dto.Build).ToEff()
            select new PublishInfo(name, store, derivation, outPath, build)
        )
        select publishInfo;

    public Aff<RT, StorePath> GetDerivation(StorePath path) =>
        from _10 in Eff(fun(() => logger.LogTrace("Resolve derivation for \"{path}\"", path)))
        from cliResult in Aff(async (RT rt) => await Cli.Wrap("nix")
            .WithArguments([
                "path-info",
                "--derivation",
                path.Value,
            ])
            .ExecuteBufferedAsync(rt.CancellationToken))
        let derivation = StorePath.From(cliResult.StandardOutput.Trim())
        select derivation;

    private Aff<RT, BufferedCommandResult> CliNix(string command, IReadOnlyList<string> args) =>
        Aff(async (RT rt) => await Cli.Wrap(command)
            .WithArguments(args)
            // .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
            .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
            .ExecuteBufferedAsync(rt.CancellationToken));
}
