using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Arrivin.Client.Application;
using Arrivin.Client.Cli;
using Arrivin.Client.GraphQL;
using Arrivin.Domain;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddTransient<CommandActions>();

services.AddApplicationServices();

var parseResult = Commands.Root.Parse(args);
if (parseResult.Errors.Count > 0)
{
    foreach (var parseError in parseResult.Errors)
    {
        Console.Error.WriteLine(parseError.Message);
    }

    return -1;
}

services.AddGraphQLServices(new Uri(parseResult.GetValue(Options.Server)!));

await using var serviceProvider = services.BuildServiceProvider();
serviceProvider.GetRequiredService<CommandActions>().Init();
return await parseResult.InvokeAsync();