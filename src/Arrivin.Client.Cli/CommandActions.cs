using System.CommandLine;
using Arrivin.Client.Application;
using Arrivin.Client.Domain;
using Arrivin.Domain;
using LanguageExt;
using LanguageExt.Sys.Live;
using LanguageExt.UnsafeValueAccess;
using Vogen;
using static LanguageExt.Prelude;

namespace Arrivin.Client.Cli;

public class CommandActions(
    GetDeployment<Runtime> getDeployment,
    SetDeployment<Runtime> setDeployment,
    PushDeployment<Runtime> pushDeployment,
    PullDeployment<Runtime> pullDeployment,
    PublishDeployment<Runtime> publishDeployment,
    DeployDeployment<Runtime> deployDeployment
)
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

    private static Eff<T> FromValueObjectValidation<T>(ValueObjectOrError<T> valueOrError) =>
        valueOrError.IsSuccess ? SuccessEff(valueOrError.ValueObject) : FailEff<T>(valueOrError.Error.ErrorMessage);

    private Aff<Runtime, Unit> Get(ParseResult parseResult) =>
        from serverUrl in ArgEff.Server(parseResult)
        from name in GetRequiredValue(parseResult, Arguments.DeploymentName)
            .Bind(v => FromValueObjectValidation(DeploymentName.TryFrom(v)))
        from deploymentInfo in getDeployment.For(serverUrl, name)
        from _ in Eff(fun(() => Console.WriteLine(deploymentInfo)))
        select unit;
    
    private Aff<Runtime, Unit> Push(ParseResult parseResult) =>
        from serverUrl in ArgEff.Server(parseResult)
        from name in GetRequiredValue(parseResult, Arguments.DeploymentName)
            .Bind(v => FromValueObjectValidation(DeploymentName.TryFrom(v)))
        from storeUrl in GetRequiredValue(parseResult, Options.Store)
            .Bind(v => FromValueObjectValidation(StoreUrl.TryFrom(new Uri(v))))
        from path in GetRequiredValue(parseResult, Arguments.Path)
            .Bind(v => FromValueObjectValidation(StorePath.TryFrom(v)))
        from _ in pushDeployment.With(serverUrl, name, storeUrl, path)
        select unit;
    
    private Aff<Runtime, Unit> Pull(ParseResult parseResult) =>
        from serverUrl in ArgEff.Server(parseResult)
        from name in GetRequiredValue(parseResult, Arguments.DeploymentName)
            .Bind(v => FromValueObjectValidation(DeploymentName.TryFrom(v)))
        from _ in pullDeployment.With(serverUrl, name)
        select unit;
    
    private Aff<Runtime, Unit> Publish(ParseResult parseResult) =>
        from serverUrl in ArgEff.Server(parseResult)
        from installable in GetRequiredValue(parseResult, Arguments.Installable)
            .Bind(v => FromValueObjectValidation(Installable.TryFrom(v)))
        from ignorePushErrors in GetRequiredValue(parseResult, Options.IgnorePushErrors)
        from _ in publishDeployment.With(serverUrl, installable, ignorePushErrors)
        select unit;
    
    private Aff<Runtime, Unit> Deploy(ParseResult parseResult) =>
        from serverUrl in ArgEff.Server(parseResult)
        from name in GetRequiredValue(parseResult, Arguments.DeploymentName)
            .Bind(v => FromValueObjectValidation(DeploymentName.TryFrom(v)))
        from _ in deployDeployment.With(serverUrl, name)
        select unit;

    private static Eff<T> GetRequiredValue<T>(ParseResult parseResult, Argument<T> argument) =>
        Eff(() => parseResult.GetRequiredValue(argument));

    private static Eff<T> GetRequiredValue<T>(ParseResult parseResult, System.CommandLine.Option<T> option) =>
        Eff(() => parseResult.GetRequiredValue(option));

    private static Eff<LanguageExt.Option<T>> GetValue<T>(ParseResult parseResult, System.CommandLine.Option<T> argument) =>
        Eff(() => Optional(parseResult.GetValue(argument)));

    private static Func<ParseResult, CancellationToken, Task<int>> RunEff(Func<ParseResult, Aff<Runtime, Unit>> fn) =>
        async (parseResult, cancellationToken) =>
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var runtime = Runtime.New(cts);
            var result = await fn(parseResult).Run(runtime);
            return result.Match(_ => 0, e =>
            {
                Console.Error.WriteLine(e.ToErrorException());
                return e.Code;
            });
        };

    private Aff<Runtime, Unit> Set(ParseResult parseResult) =>
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
        public static Eff<ServerUrl> Server(ParseResult parseResult) =>
            GetRequiredValue(parseResult, Options.Server)
                .Bind(v => FromValueObjectValidation(ServerUrl.TryFrom(new Uri(v))));
    }
}
