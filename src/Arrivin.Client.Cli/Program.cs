using Arrivin.Client.Application;
using Arrivin.Client.Cli;
using Arrivin.Client.GraphQL;
using Arrivin.Client.NixCli;
using LanguageExt.Sys.Live;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddTransient<CommandActions>();
services.AddApplicationServices<Runtime>();
services.AddGraphQLServices<Runtime>();
services.AddNixCli<Runtime>();

await using var serviceProvider = services.BuildServiceProvider();
serviceProvider.GetRequiredService<CommandActions>().Init();

var parseResult = Commands.Root.Parse(args);
return await parseResult.InvokeAsync();
