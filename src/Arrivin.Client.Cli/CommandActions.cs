using System.CommandLine;
using Arrivin.Client.Application;
using Arrivin.Domain;
using static LanguageExt.Prelude;

namespace Arrivin.Client.Cli;

public class CommandActions(GetDeployment getDeployment, SetDeployment setDeployment)
{
    public void Init()
    {
        Commands.Get.SetAction(Get);
        Commands.Set.SetAction(Set);
    }

    private async Task Get(ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (DeploymentName.TryFrom(parseResult.GetValue(Arguments.DeploymentName)!) is not { ValueObject: { } name })
            throw new Exception();
        var deploymentInfo = await getDeployment.For(name, cancellationToken);
        Console.WriteLine(deploymentInfo);
    }

    private async Task Set(ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (DeploymentName.TryFrom(parseResult.GetValue(Arguments.DeploymentName)!) is not { ValueObject: { } name })
            throw new Exception();
        if (StoreUrl.TryFrom(new Uri(parseResult.GetValue(Options.Store)!)) is not { ValueObject: { } storeUrl })
            throw new Exception();
        if (StorePath.TryFrom(parseResult.GetValue(Options.Derivation)!) is not { ValueObject: { } derivation })
            throw new Exception();
        var outPath = Optional(parseResult.GetValue(Options.OutPath))
            .Map(v => StorePath.TryFrom(v) is not {ValueObject: { } outPath} ? throw new Exception() : outPath)
            .IfNoneUnsafe((StorePath?) null);
        var deploymentInfo = new DeploymentInfo(storeUrl, derivation, outPath);
        await setDeployment.For(name, deploymentInfo, cancellationToken);
    }
}