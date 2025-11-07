using System.CommandLine;

namespace Arrivin.Client.Cli;

public static class Options
{
    public static Option<DirectoryInfo> DataDirectory { get; } = new("--data-directory")
    {
        DefaultValueFactory = _ => new DirectoryInfo("/var/lib/arrivin"),
    };

    public static Option<string> Derivation { get; } = new("--derivation", "-d");

    public static Option<bool> IgnorePushErrors { get; } = new("--ignore-push-errors");

    public static Option<string> OutPath { get; } = new("--out-path", "-o");

    public static Option<string> Server { get; } = new("--server", "-u");

    public static Option<string> Store { get; } = new("--store", "-s");
    
    public static Option<string> UseStore { get; } = new("--use-store");
}
