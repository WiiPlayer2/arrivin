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
        from filteredExtraArgs in Eff(() => NixArgs.From(extraArgs.Value.Except(["--offline"]).ToList()))
        from _10 in Eff(fun(() => logger.LogTrace("Building \"{derivation}\" using extra args: {extraArgs}", derivation, filteredExtraArgs)))
        from result in CliNix("nix-store", [
            "--realise",
            derivation.Value,
            ..filteredExtraArgs.Value,
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
        from enrichedExtraArgs in EnrichNixArgs(installable, extraArgs)
        from result in CliNix("nix", ["eval", "--json", installable.Value, ..enrichedExtraArgs.Value])
        from dto in Eff(() => JsonSerializer.Deserialize<PublishInfoDto>(result.StandardOutput))
        from publishInfo in (
            from name in Optional(dto.Name).ToEff().Map(DeploymentName.From)
            from derivation in Optional(dto.Derivation).ToEff().Map(StorePath.From)
            from outPath in Optional(dto.OutPath).ToEff().Map(StorePath.From)
            from build in Optional(dto.Build).ToEff()
            select new PublishInfo(name, derivation, outPath, build)
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
        from cli in Eff(() => Cli.Wrap(command)
            .WithArguments(args)
            // .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
            .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError())))
        from _ in Eff(fun(() => logger.LogTrace("+{exe} {args}", cli.TargetFilePath, cli.Arguments)))
        from result in Aff(async (RT rt) => await cli
            .ExecuteBufferedAsync(rt.CancellationToken))
        select result;

    private Aff<RT, NixArgs> EnrichNixArgs(Installable installable, NixArgs extraArgs) =>
        from result in CliNix("nix", ["eval", "--json", $"{installable}.impure", ..extraArgs.Value])
        from needImpure in Eff(() => JsonSerializer.Deserialize<bool>(result.StandardOutput))
        let enrichedArgs = needImpure
            ? NixArgs.From(["--impure", ..extraArgs.Value])
            : extraArgs
        select enrichedArgs;
}
