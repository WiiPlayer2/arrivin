{ testers
, inputs
, outputs
, stdenv
, hello
, lib
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

    client_push = {
      imports = [
        commonMachineModule
        clientMachineModule
      ];

      system.extraDependencies = [
        arrivinJob.drv
        arrivinJob.out
      ];

      services.arrivin.client = {
        enable = true;

        publish = {
          enable = true;
          jobs = [ ];
          remote = "<TODO>";
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
          jobs = [ ];
        };
      };
    };
  };
  testScript = ''
    start_all()
    server.wait_for_unit("default.target")
    client_push.wait_for_unit("default.target")
    client_deploy.wait_for_unit("default.target")
    client_push.succeed("arrivin -u http://server:5014/ push ${arrivinJob.name} ${arrivinJob.out}")
    print(client_deploy.succeed("arrivin -u http://server:5014/ deploy ${arrivinJob.name}"))
  '';
}
