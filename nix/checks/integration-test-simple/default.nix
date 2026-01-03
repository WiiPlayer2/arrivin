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
        url = "http://localhost:5014/graphql";
      };
    };

  arrivinJob = outputs.lib.cfg {
    name = "test";
    store = "unix:///nix/var/nix/daemon-socket/socket";
    path = outputs.lib.${stdenv.system}.activate.custom hello ''
      ${getExe hello}
    '';
  };
in
testers.runNixOSTest {
  name = "integration-test-simple";
  nodes = {
    machine = {
      imports = [
        commonMachineModule
        clientMachineModule
      ];

      system.extraDependencies = [
        arrivinJob.drv
        arrivinJob.out
      ];

      services.arrivin = {
        server = {
          enable = true;
        };

        client = {
          enable = true;

          publish = {
            enable = true;
            jobs = [ ];
            remote = "<TODO>";
          };

          deploy = {
            enable = true;
            jobs = [ ];
          };
        };
      };
    };
  };
  testScript = ''
    machine.wait_for_unit("arrivind.service")
    machine.wait_for_unit("default.target")
    machine.succeed("arrivin -u http://localhost:5014/graphql push ${arrivinJob.name} ${arrivinJob.out} --store ${arrivinJob.store}")
    print(machine.succeed("arrivin -u http://localhost:5014/graphql deploy ${arrivinJob.name}"))
  '';
}
