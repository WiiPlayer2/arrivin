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
}
