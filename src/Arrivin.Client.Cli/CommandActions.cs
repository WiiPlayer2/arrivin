using System.CommandLine;
using Arrivin.Client.Application;
using Arrivin.Domain;

namespace Arrivin.Client.Cli;

public class CommandActions(GetDeployment getDeployment)
{
    public void Init()
    {
        Commands.Get.SetAction(Get);
    }

    private async Task Get(ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (DeploymentName.TryFrom(parseResult.GetValue(Arguments.DeploymentName)!) is not { ValueObject: { } name })
            throw new Exception();
        var deploymentInfo = await getDeployment.For(name, cancellationToken);
        Console.WriteLine(deploymentInfo);
    }
}