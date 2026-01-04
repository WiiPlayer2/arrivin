using System.CommandLine;

namespace Arrivin.Client.Cli;

public static class Commands
{
    public static Command Deploy { get; } = new("deploy")
    {
        Arguments.DeploymentName,
        Arguments.ExtraArgs,
    };

    public static Command Get { get; } = new("get")
    {
        Arguments.DeploymentName,
    };

    public static Command Publish { get; } = new("publish")
    {
        Options.IgnorePushErrors,
        Arguments.Installable,
        Arguments.ExtraArgs,
    };

    public static Command Pull { get; } = new("pull")
    {
        Arguments.DeploymentName,
    };

    public static Command Push { get; } = new("push")
    {
        Arguments.DeploymentName,
        Arguments.Path,
    };

    public static RootCommand Root => new()
    {
        Options.Server,
        Options.DataDirectory,
        Get,
        Set,
        Push,
        Pull,
        Publish,
        Deploy,
    };

    public static Command Set { get; } = new("set")
    {
        Arguments.DeploymentName,
        Options.Derivation,
        Options.OutPath,
    };
}
