using Arrivin.Server.Web;
using Arrivin.Domain;
using Arrivin.Server.Application;
using Arrivin.Server.InMemory;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGraphQLServer()
	.AddType<Query>()
	.BindRuntimeType<StorePath, StringType>()
	.AddTypeConverter<StorePath, string>(x => x.Value)
	.AddTypeConverter<string, StorePath>(StorePath.From)
	.BindRuntimeType<DeploymentName, StringType>()
	.AddTypeConverter<DeploymentName, string>(x => x.Value)
	.AddTypeConverter<string, DeploymentName>(DeploymentName.From)
	.BindRuntimeType<StoreUrl, UrlType>()
	.AddTypeConverter<StoreUrl, Uri>(x => x.Value)
	.AddTypeConverter<Uri, StoreUrl>(StoreUrl.From);

builder.Services.AddApplicationServices();
builder.Services.AddInMemoryServices();

var app = builder.Build();
app.MapGraphQL();
app.Run();
