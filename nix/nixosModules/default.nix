{ lib, config, pkgs, ... }:
with lib;
let
  cfg = config.services.arrivin;
  format = pkgs.formats.json { };
  serverConfigFile = format.generate "arrivind.json" cfg.server.settings;
in
{
  options.services.arrivin = {
    server = {
      enable = mkEnableOption "";
      package = mkPackageOption pkgs "arrivind" {};
      configFile = mkOption {
        type = types.path;
        default = "${serverConfigFile}";
      };
      settings = mkOption {
        type = format.type;
      };
      openFirewall = mkOption {
        type = types.bool;
        default = true;
      };
      config = {
        listen = {
          host = mkOption {
            type = types.str;
            default = "0.0.0.0";
          };
          port = mkOption {
            type = types.port;
            default = 5014; # pending
          };
        };
      };
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
        environment.ARRIVIND_CONFIG = cfg.server.configFile;
        script = ''
          ${getExe cfg.server.package}
        '';
      };

      services.arrivin.server.settings.Kestrel.Endpoints.Http.Url = "http://${cfg.server.config.listen.host}:${toString cfg.server.config.listen.port}";

      networking.firewall.allowedTCPPorts = mkIf cfg.server.openFirewall [ cfg.server.config.listen.port ];
    })
    (mkIf cfg.client.enable {
      environment.systemPackages = [ cfg.client.package ];
    })
  ];
}
