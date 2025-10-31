{
  buildDotnetModule,

  dotnetCorePackages,
}:
buildDotnetModule {
  pname = "arrivind";
  version = "0.1";
  src = ./../../../src;

  projectFile = "Arrivin.Server.Web/Arrivin.Server.Web.csproj";
  nugetDeps = ./deps.json;

  dotnet-runtime = dotnetCorePackages.aspnetcore_8_0;

  meta = {
    mainProgram = "arrivind";
  };
}
