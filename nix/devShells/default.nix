{
  mkShell,
  
  git-ignore,
  license-cli,
  debase,
  gitui,
  md-tui,
  
  omnisharp-roslyn,
  netcoredbg,
  dotnetCorePackages,
}:
mkShell {
  name = "arrivin-dev";
  packages = [
    git-ignore
    license-cli
    debase
    gitui
    md-tui
    
    omnisharp-roslyn
    netcoredbg
    dotnetCorePackages.dotnet_9.sdk
  ];
}
