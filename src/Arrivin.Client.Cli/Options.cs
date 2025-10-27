using System.CommandLine;

namespace Arrivin.Client.Cli;

public static class Options
{
    public static Option<string> Server { get; } = new("server", "-u", "--server");

    public static Option<string> Store { get; } = new("store", "-s", "--store");

    public static Option<string> Derivation { get; } = new("derivation", "-d", "--derivation");

    public static Option<string> OutPath { get; } = new("out-path", "-o", "--out-path");
}