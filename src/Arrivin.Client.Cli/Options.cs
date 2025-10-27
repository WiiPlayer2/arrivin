using System.CommandLine;

namespace Arrivin.Client.Cli;

public static class Options
{
    public static Option<string> Server { get; } = new("server", ["-s", "--server"]);
}