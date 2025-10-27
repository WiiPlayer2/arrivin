using Arrivin.Client.Application;
using Arrivin.Client.Cli;
using Arrivin.Client.GraphQL;
using LanguageExt.Sys.Live;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddTransient<CommandActions>();

services.AddApplicationServices<Runtime>();

var parseResult = Commands.Root.Parse(args);
if (parseResult.Errors.Count > 0)
{
    foreach (var parseError in parseResult.Errors)
    {
        Console.Error.WriteLine(parseError.Message);
    }

    return -1;
}

services.AddGraphQLServices<Runtime>(new Uri(parseResult.GetValue(Options.Server)!));

await using var serviceProvider = services.BuildServiceProvider();
serviceProvider.GetRequiredService<CommandActions>().Init();
return await parseResult.InvokeAsync();
