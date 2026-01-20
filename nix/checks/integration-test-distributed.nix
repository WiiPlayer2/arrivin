{ testers
, inputs
, outputs
, stdenv
, hello
, lib
, runCommand
, git
, writeText
, ...
}:
with lib;
let
  commonMachineModule =
    {
      imports = [
        inputs.self.nixosModules.arrivin
      ];

      nix.settings.experimental-features = [
        "nix-command"
        "flakes"
      ];
    };

  clientMachineModule =
    {
      services.arrivin.client = {
        url = "http://server:5014/";
      };
    };

  arrivinJob = outputs.lib.cfg {
    name = "test";
    path = outputs.lib.${stdenv.system}.activate.custom hello ''
      ${getExe hello}
    '';
  };

  repoPkg =
    let
      flakeFile = writeText "flake.nix" ''
        {
          inputs = {
            arrivin.url = "path:${inputs.self}";
            arrivin.inputs.flakelight.follows = "flakelight";
            arrivin.inputs.nixpkgs.follows = "nixpkgs";
            nixpkgs.url = "path:${inputs.nixpkgs}";
            flakelight.url = "path:${inputs.flakelight}";
            flakelight.inputs.nixpkgs.follows = "nixpkgs";
          };

          outputs = { nixpkgs, arrivin, ... }: {
            arrivin.test = arrivin.lib.cfg {
              name = "test";
              path = arrivin.lib.${stdenv.system}.activate.custom nixpkgs.legacyPackages.${stdenv.system}.hello '''
                ''${nixpkgs.lib.getExe nixpkgs.legacyPackages.${stdenv.system}.hello}
              ''';
            };
          };
        }
      '';
    in
    runCommand "repo"
      {
        buildInputs = [
          git
        ];
      } ''
      mkdir -p $out
      cd $out

      git init -b main .
      git config user.email "test@arrivin"
      git config user.name "Arrivin"

      cp ${flakeFile} flake.nix
      git add flake.nix
      git commit -m "Add flake.nix"
    '';
in
testers.runNixOSTest {
  name = "integration-test-distributed";
  nodes = {
    server = {
      imports = [
        commonMachineModule
      ];

      services.arrivin = {
        server = {
          enable = true;
          settings.Logging.LogLevel."Arrivin.Server.Web.StoreController" = "Trace";
        };
      };
    };

    client_publish = {
      imports = [
        commonMachineModule
        clientMachineModule
      ];

      system.extraDependencies = [
        arrivinJob.drv
        arrivinJob.out
      ];

      environment.systemPackages = [
        git
      ];

      services.arrivin.client = {
        enable = true;

        publish = {
          enable = true;
          jobs = [
            "test"
          ];
          remote = "${repoPkg}";
          extraArgs = [
            "--offline"
          ];
        };
      };
    };

    client_deploy = {
      imports = [
        commonMachineModule
        clientMachineModule
      ];

      services.arrivin.client = {
        enable = true;

        deploy = {
          enable = true;
          jobs = [
            "test"
          ];
        };
      };
    };
  };
  testScript = ''
    start_all()
    server.wait_for_unit("default.target")
    client_publish.wait_for_unit("default.target")
    client_deploy.wait_for_unit("default.target")

    client_publish.systemctl("start arrivin-publish.service")
    client_publish.wait_for_console_text("arrivin-publish.service: Deactivated successfully.")

    client_deploy.systemctl("start arrivin-deploy.service")
    client_deploy.wait_for_console_text("Hello, world!")
    client_deploy.wait_for_console_text("arrivin-deploy.service: Deactivated successfully.")
  '';
}
