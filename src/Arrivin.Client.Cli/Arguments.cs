using System.CommandLine;

namespace Arrivin.Client.Cli;

public static class Arguments
{
    public static Argument<string> DeploymentName { get; } = new("deployment");
    
    public static Argument<string> Path { get; } = new("path");
    
    public static Argument<string> Installable { get; } = new("installable");
}