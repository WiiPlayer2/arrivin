using System.Text.Json;
using Arrivin.Client.Application;
using Arrivin.Domain;
using CliWrap;
using CliWrap.Buffered;
using LanguageExt;
using LanguageExt.Effects.Traits;
using static LanguageExt.Prelude;

namespace Arrivin.Client.NixCli;

public class NixCli<RT> : INix<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> CopyTo(StoreUrl store, StorePath path) =>
        Aff(async (RT rt) => await Cli.Wrap("nix")
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
        ).Map(_ => unit);

    public Aff<RT, Unit> CopyFrom(StoreUrl store, StorePath path) =>
        Aff(async (RT rt) => await Cli.Wrap("nix")
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
        ).Map(_ => unit);

    public Aff<RT, StorePath> GetDerivation(StorePath path) =>
        from cliResult in Aff(async (RT rt) => await Cli.Wrap("nix")
            .WithArguments([
                "path-info",
                "--derivation",
                path.Value,
            ])
            .ExecuteBufferedAsync(rt.CancellationToken))
        let derivation = StorePath.From(cliResult.StandardOutput.Trim())
        select derivation;

    public Aff<RT, PublishInfo> EvaluateDeployment(Installable installable) =>
        from result in CliNix("nix", ["eval", "--json", installable.Value])
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

    public Aff<RT, Unit> Build(StorePath derivation) => CliNix("nix-store", [
        "--realise",
        derivation.Value,
    ]).Map(_ => unit);

    private Aff<RT, BufferedCommandResult> CliNix(string command, IReadOnlyList<string> args) =>
        Aff(async (RT rt) => await Cli.Wrap(command)
            .WithArguments(args)
            // .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
            .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
            .ExecuteBufferedAsync(rt.CancellationToken));
}