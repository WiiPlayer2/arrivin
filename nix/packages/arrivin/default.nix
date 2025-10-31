{
  buildDotnetModule,

  dotnetCorePackages,
}:
buildDotnetModule {
  pname = "arrivin";
  version = "0.1";
  src = ./../../../src;

  projectFile = "Arrivin.Client.Cli/Arrivin.Client.Cli.csproj";
  nugetDeps = ./deps.json;

  dotnet-runtime = dotnetCorePackages.runtime_8_0;

  meta = {
    mainProgram = "arrivin";
  };
}
