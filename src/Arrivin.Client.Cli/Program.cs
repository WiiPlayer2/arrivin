using Arrivin.Client.Application;
using Arrivin.Client.Cli;
using Arrivin.Client.GraphQL;
using Arrivin.Client.NixCli;
using Arrivin.Client.World;
using LanguageExt.Effects.Traits;
using LanguageExt.Sys.Live;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();
services.AddSingleton(new Runner<Runtime>(ct =>
{
    var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    var runtime = Runtime.New(cts);
    return (runtime, cts);
}));
AddServices<Runtime>(services);

services.AddLogging(logging => logging
    .SetMinimumLevel(LogLevel.Trace)
    .AddConsole());

await using var serviceProvider = services.BuildServiceProvider();
serviceProvider.GetRequiredService<CommandActions<Runtime>>().Init();

var parseResult = Commands.Root.Parse(args);
return await parseResult.InvokeAsync();

static void AddServices<RT>(IServiceCollection services) where RT : struct, HasCancel<RT>
{
    services.AddTransient<CommandActions<RT>>();
    services.AddApplicationServices<RT>();
    services.AddGraphQLServices<RT>();
    services.AddNixCli<RT>();
    services.AddWorldServices<RT>();
}
