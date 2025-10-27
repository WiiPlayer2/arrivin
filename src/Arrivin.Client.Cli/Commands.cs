using System.CommandLine;
using Arrivin.Domain;

namespace Arrivin.Client.Cli;

public static class Commands
{
    public static RootCommand Root => new()
    {
        Options.Server,
        Get,
        Set,
    };

    public static Command Get { get; } = new("get")
    {
        Arguments.DeploymentName,
    };

    public static Command Set { get; } = new("set")
    {
        Arguments.DeploymentName,
        Options.Store,
        Options.Derivation,
        Options.OutPath,
    };
}