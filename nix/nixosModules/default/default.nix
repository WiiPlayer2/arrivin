{ outputs, ... }:
{ lib, config, pkgs, ... }:
with lib;
let
  cfg = config.services.arrivin;
  format = pkgs.formats.json { };
  serverConfigFile = format.generate "arrivind.json" cfg.server.settings;

  # client publish
  runPublishScript = pkgs.writeShellApplication {
    name = "run-publish";
    runtimeInputs = with pkgs; [
      git
      cfg.client.package
    ];
    text = readFile ./run-publish.sh;
  };
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
      debug = mkOption {
        type = types.bool;
        default = false;
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
        dataDir = mkOption {
          type = types.str;
          default = "/var/lib/arrivin";
        };
      };
    };
    client = {
      enable = mkEnableOption "";
      package = mkPackageOption pkgs "arrivin" {};

      url = mkOption {
        type = types.str;
      };

      publish = {
        enable = mkEnableOption "";

        remote = mkOption {
          type = types.str;
        };

        jobs = mkOption {
          type = with types; listOf str;
        };
      };

      deploy = {
        enable = mkEnableOption "";

        jobs = mkOption {
          type = with types; listOf str;
        };
      };
    };
  };

  config = mkMerge [
    {
      nixpkgs.overlays = [
        outputs.overlays.default
      ];
    }
    (mkIf cfg.server.enable {
      environment.systemPackages = [ cfg.server.package ];

      systemd.services.arrivind = {
        environment = {
          ARRIVIND_CONFIG = cfg.server.configFile;
          ASPNETCORE_ENVIRONMENT = mkIf cfg.server.debug "Development";
        };
        script = ''
          ${getExe cfg.server.package}
        '';
      };

      services.arrivin.server.settings = {
        Kestrel.Endpoints.Http.Url = "http://${cfg.server.config.listen.host}:${toString cfg.server.config.listen.port}";
        FileStore.Path = "${cfg.server.config.dataDir}/deployments";
      };

      networking.firewall.allowedTCPPorts = mkIf cfg.server.openFirewall [ cfg.server.config.listen.port ];
    })
    (mkIf cfg.client.enable {
      environment.systemPackages = [ cfg.client.package ];
    })
    (mkIf (cfg.client.enable && cfg.client.publish.enable) {
      systemd = {
        timers.arrivin-publish = {
          timerConfig = {
            OnBootSec = "10 min";
            OnUnitInactiveSec = "2 h";
          };
          wantedBy = ["multi-user.target"];
        };

        services.arrivin-publish = {
          script = "${getExe runPublishScript} \"$@\"";
          scriptArgs = escapeShellArgs ([
              cfg.client.url
              "/var/lib/arrivin"
              cfg.client.publish.remote
            ] ++ cfg.client.publish.jobs);
        };
      };
    })
  ];
}
