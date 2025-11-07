using System.CommandLine;
using Arrivin.Client.Application;
using Arrivin.Client.Domain;
using Arrivin.Domain;
using LanguageExt;
using LanguageExt.Effects.Traits;
using LanguageExt.UnsafeValueAccess;
using Vogen;
using static LanguageExt.Prelude;

namespace Arrivin.Client.Cli;

public class CommandActions<RT>(
    Runner<RT> runner,
    GetDeployment<RT> getDeployment,
    SetDeployment<RT> setDeployment,
    PushDeployment<RT> pushDeployment,
    PullDeployment<RT> pullDeployment,
    PublishDeployment<RT> publishDeployment,
    DeployDeployment<RT> deployDeployment
) where RT : struct, HasCancel<RT>
{
    public void Init()
    {
        Commands.Get.SetAction(RunEff(Get));
        Commands.Set.SetAction(RunEff(Set));
        Commands.Push.SetAction(RunEff(Push));
        Commands.Pull.SetAction(RunEff(Pull));
        Commands.Publish.SetAction(RunEff(Publish));
        Commands.Deploy.SetAction(RunEff(Deploy));
    }

    private Aff<RT, Unit> Deploy(ParseResult parseResult) =>
        from serverUrl in ArgEff.Server(parseResult)
        from name in GetRequiredValue(parseResult, Arguments.DeploymentName)
            .Bind(v => FromValueObjectValidation(DeploymentName.TryFrom(v)))
        from extraBuildArgs in ArgEff.ExtraArgs(parseResult)
        from dataDirectory in ArgEff.DataDirectory(parseResult)
        from _ in deployDeployment.With(serverUrl, dataDirectory, name, extraBuildArgs)
        select unit;

    private static Eff<T> FromValueObjectValidation<T>(ValueObjectOrError<T> valueOrError) =>
        valueOrError.IsSuccess ? SuccessEff(valueOrError.ValueObject) : FailEff<T>(valueOrError.Error.ErrorMessage);

    private Aff<RT, Unit> Get(ParseResult parseResult) =>
        from serverUrl in ArgEff.Server(parseResult)
        from name in GetRequiredValue(parseResult, Arguments.DeploymentName)
            .Bind(v => FromValueObjectValidation(DeploymentName.TryFrom(v)))
        from deploymentInfo in getDeployment.For(serverUrl, name)
        from _ in Eff(fun(() => Console.WriteLine(deploymentInfo)))
        select unit;

    private static Eff<T> GetRequiredValue<T>(ParseResult parseResult, Argument<T> argument) =>
        Eff(() => parseResult.GetRequiredValue(argument));

    private static Eff<T> GetRequiredValue<T>(ParseResult parseResult, System.CommandLine.Option<T> option) =>
        Eff(() => parseResult.GetRequiredValue(option));

    private static Eff<LanguageExt.Option<T>> GetValue<T>(ParseResult parseResult, System.CommandLine.Option<T> argument) =>
        Eff(() => Optional(parseResult.GetValue(argument)));

    private Aff<RT, Unit> Publish(ParseResult parseResult) =>
        from serverUrl in ArgEff.Server(parseResult)
        from installable in GetRequiredValue(parseResult, Arguments.Installable)
            .Bind(v => FromValueObjectValidation(Installable.TryFrom(v)))
        from ignorePushErrors in GetRequiredValue(parseResult, Options.IgnorePushErrors)
        from extraBuildArgs in ArgEff.ExtraArgs(parseResult)
        from _ in publishDeployment.With(serverUrl, installable, ignorePushErrors, extraBuildArgs)
        select unit;

    private Aff<RT, Unit> Pull(ParseResult parseResult) =>
        from serverUrl in ArgEff.Server(parseResult)
        from name in GetRequiredValue(parseResult, Arguments.DeploymentName)
            .Bind(v => FromValueObjectValidation(DeploymentName.TryFrom(v)))
        from _ in pullDeployment.With(serverUrl, name)
        select unit;

    private Aff<RT, Unit> Push(ParseResult parseResult) =>
        from serverUrl in ArgEff.Server(parseResult)
        from name in GetRequiredValue(parseResult, Arguments.DeploymentName)
            .Bind(v => FromValueObjectValidation(DeploymentName.TryFrom(v)))
        from storeUrl in GetRequiredValue(parseResult, Options.Store)
            .Bind(v => FromValueObjectValidation(StoreUrl.TryFrom(new Uri(v))))
        from path in GetRequiredValue(parseResult, Arguments.Path)
            .Bind(v => FromValueObjectValidation(StorePath.TryFrom(v)))
        from _ in pushDeployment.With(serverUrl, name, storeUrl, path)
        select unit;

    private Func<ParseResult, CancellationToken, Task<int>> RunEff(Func<ParseResult, Aff<RT, Unit>> fn) =>
        async (parseResult, cancellationToken) =>
        {
            var result = await runner.Run(fn(parseResult), cancellationToken);
            return result.Match(_ => 0, e =>
            {
                Console.Error.WriteLine(e.ToErrorException());
                return e.Code;
            });
        };

    private Aff<RT, Unit> Set(ParseResult parseResult) =>
        from serverUrl in ArgEff.Server(parseResult)
        from name in GetRequiredValue(parseResult, Arguments.DeploymentName)
            .Bind(v => FromValueObjectValidation(DeploymentName.TryFrom(v)))
        from storeUrl in GetRequiredValue(parseResult, Options.Store)
            .Bind(v => FromValueObjectValidation(StoreUrl.TryFrom(new Uri(v))))
        from derivation in GetRequiredValue(parseResult, Options.Derivation)
            .Bind(v => FromValueObjectValidation(StorePath.TryFrom(v)))
        from outPathOption in GetValue(parseResult, Options.OutPath)
            .Bind(vOption => vOption.Map(v => FromValueObjectValidation(StorePath.TryFrom(v))).Traverse(_ => _))
        let deploymentInfo = new DeploymentInfo(storeUrl, derivation, outPathOption.ValueUnsafe())
        from _ in setDeployment.For(serverUrl, name, deploymentInfo)
        select unit;

    private static class ArgEff
    {
        public static Eff<FilePath> DataDirectory(ParseResult parseResult) =>
            GetRequiredValue(parseResult, Options.DataDirectory)
                .Bind(v => FromValueObjectValidation(FilePath.TryFrom(v.FullName)));

        public static Eff<NixArgs> ExtraArgs(ParseResult parseResult) =>
            GetRequiredValue(parseResult, Arguments.ExtraArgs)
                .Bind(v => FromValueObjectValidation(NixArgs.TryFrom(v)));

        public static Eff<ServerUrl> Server(ParseResult parseResult) =>
            GetRequiredValue(parseResult, Options.Server)
                .Bind(v => FromValueObjectValidation(ServerUrl.TryFrom(new Uri(v))));
    }
}
