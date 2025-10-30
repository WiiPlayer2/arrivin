{ lib, genSystems, ... }:
with lib;
genSystems (pkgs:
let
  # based on https://github.com/serokell/deploy-rs/blob/master/flake.nix
  activate = {
    custom = base: activate:
      pkgs.buildEnv {
        name = ("activatable-" + base.name);
        paths =
          [
            base
            (pkgs.writeTextFile {
              name = base.name + "-activate-path";
              text = ''
                #!${pkgs.runtimeShell}
                set -euo pipefail

                ${activate}
              '';
              executable = true;
              destination = "/arrivin-activate";
            })
          ];
      };

    nixos = base:
      activate.custom
      base.config.system.build.toplevel
      ''
        # work around https://github.com/NixOS/nixpkgs/issues/73404
        cd /tmp

        $PROFILE/bin/switch-to-configuration switch

        # https://github.com/serokell/deploy-rs/issues/31
        ${with base.config.boot.loader;
        optionalString systemd-boot.enable
        "sed -i '/^default /d' ${efi.efiSysMountPoint}/loader/loader.conf"}
      '';
  };
in
{
  inherit activate;
})
