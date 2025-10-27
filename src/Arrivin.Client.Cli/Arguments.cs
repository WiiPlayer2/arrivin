using System.CommandLine;

namespace Arrivin.Client.Cli;

public static class Arguments
{
    public static Argument<string> DeploymentName { get; } = new("deployment");
}