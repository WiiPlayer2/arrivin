using Arrivin.Domain;
using Arrivin.Server.Application;
using Arrivin.Server.InMemory;
using Arrivin.Server.Web;
using LanguageExt;
using LanguageExt.Sys.Live;

var builder = WebApplication.CreateBuilder(args);
if(Environment.GetEnvironmentVariable("ARRIVIND_CONFIG") is {} configPath)
    builder.Configuration.AddJsonFile(configPath, true);
builder.Services.AddGraphQLServer()
    .AddQueryType<Query<Runtime>>()
    .AddMutationType<Mutation<Runtime>>()
    .BindRuntimeType<DeploymentInfo, DeploymentInfoType>()
    .AddTypeConverter<DeploymentInfo, DeploymentInfoDto>(Mapper.Map)
    .AddTypeConverter<DeploymentInfoDto, DeploymentInfo>(Mapper.Map)
    .BindRuntimeType<Unit, UnitType>()
    .BindRuntimeType<StorePath, StringType>()
    .AddTypeConverter<StorePath, string>(x => x.Value)
    .AddTypeConverter<string, StorePath>(StorePath.From)
    .BindRuntimeType<DeploymentName, StringType>()
    .AddTypeConverter<DeploymentName, string>(x => x.Value)
    .AddTypeConverter<string, DeploymentName>(DeploymentName.From)
    .BindRuntimeType<StoreUrl, UrlType>()
    .AddTypeConverter<StoreUrl, Uri>(x => x.Value)
    .AddTypeConverter<Uri, StoreUrl>(StoreUrl.From);

builder.Services.AddSingleton(new Runner<Runtime>(ct =>
{
    var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    var runtime = Runtime.New(cts);
    return (runtime, cts);
}));
builder.Services.AddApplicationServices<Runtime>();
builder.Services.AddInMemoryServices<Runtime>();

var app = builder.Build();
app.MapGraphQL();
app.Run();
