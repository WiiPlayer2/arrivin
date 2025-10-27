using System.CommandLine;
using Arrivin.Domain;

namespace Arrivin.Client.Cli;

public static class Commands
{
    public static RootCommand Root => new()
    {
        Options.Server,
        Get,
    };

    public static Command Get { get; } = new("get")
    {
        Arguments.DeploymentName,
    };
}