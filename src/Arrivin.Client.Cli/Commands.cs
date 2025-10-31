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
        Push,
        Pull,
        Publish,
        Deploy,
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

    public static Command Push { get; } = new("push")
    {
        Arguments.DeploymentName,
        Arguments.Path,
        Options.Store,
    };

    public static Command Pull { get; } = new("pull")
    {
        Arguments.DeploymentName,
    };

    public static Command Publish { get; } = new("publish")
    {
        Arguments.Installable,
        Options.IgnorePushErrors,
    };

    public static Command Deploy { get; } = new("deploy")
    {
        Arguments.DeploymentName,
    };
}