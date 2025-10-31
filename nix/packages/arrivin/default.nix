{
  lib,
  buildDotnetModule,

  dotnetCorePackages,
  makeWrapper,
  nix,
}:
buildDotnetModule {
  pname = "arrivin";
  version = "0.1";
  src = ./../../../src;

  projectFile = "Arrivin.Client.Cli/Arrivin.Client.Cli.csproj";
  nugetDeps = ./deps.json;

  dotnet-runtime = dotnetCorePackages.runtime_8_0;

  nativeBuildInputs = [ makeWrapper ];

  makeWrapperArgs = [
    "--prefix"
    "PATH"
    ":"
    (lib.makeBinPath [ nix ])
  ];

  meta = {
    mainProgram = "arrivin";
  };
}
