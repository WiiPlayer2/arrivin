{ lib
, symlinkJoin
, mkShell
, makeWrapper
, helix
, git-ignore
, license-cli
, debase
, gitui
, md-tui
, omnisharp-roslyn
, netcoredbg
, dotnetCorePackages
, arrivin
, arrivind
,
}:
mkShell {
  name = "arrivin-dev";
  packages = [
    git-ignore
    license-cli
    # debase # broken
    gitui
    md-tui

    dotnetCorePackages.dotnet_8.sdk

    arrivin
    arrivind
  ];
}
