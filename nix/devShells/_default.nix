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
    (symlinkJoin {
      name = "${lib.getName helix}-wrapped-${lib.getVersion helix}";
      paths = [ helix ];
      preferLocalBuild = true;
      nativeBuildInputs = [ makeWrapper ];
      postBuild = ''
        wrapProgram $out/bin/hx \
          --suffix PATH : ${lib.makeBinPath [
            omnisharp-roslyn
            netcoredbg
          ]}
      '';
    })

    git-ignore
    license-cli
    debase
    gitui
    md-tui

    omnisharp-roslyn
    netcoredbg
    dotnetCorePackages.dotnet_8.sdk

    arrivin
    arrivind
  ];
}
