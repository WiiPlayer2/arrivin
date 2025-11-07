using Arrivin.Client.Application;
using Arrivin.Domain;
using CliWrap;
using LanguageExt;
using LanguageExt.Effects.Traits;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;

namespace Arrivin.Client.NixCli;

public class Cli<RT>(ILogger<Cli<RT>> logger) : ICli<RT> where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> Call(StorePath command) =>
        from _10 in Eff(fun(() => logger.LogTrace("Executing command {command}", command)))
        from _20 in Aff(async (RT rt) => await Cli.Wrap(command.Value)
                .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
                .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
                .ExecuteAsync(rt.CancellationToken))
            .Map(_ => unit)
        select unit;
}
