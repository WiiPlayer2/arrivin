{ lib, config, pkgs, ... }:
with lib;
let
  cfg = config.services.arrivin;
in
{
  options.services.arrivin = {
    server = {
      enable = mkEnableOption "";
      package = mkPackageOption pkgs "arrivind" {};
    };
    client = {
      enable = mkEnableOption "";
      package = mkPackageOption pkgs "arrivin" {};
    };
  };

  config = mkMerge [
    (mkIf cfg.server.enable {
      environment.systemPackages = [ cfg.server.package ];

      systemd.services.arrivind = {
        script = ''
          ${getExe cfg.server.package}
        '';
      };
    })
    (mkIf cfg.client.enable {
      environment.systemPackages = [ cfg.client.package ];
    })
  ];
}
