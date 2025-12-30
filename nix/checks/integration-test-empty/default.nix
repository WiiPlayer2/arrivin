{ testers
, inputs
, ...
}:
let
  commonMachineModule =
    {
      imports = [
        inputs.self.nixosModules.arrivin
      ];
    };

  clientMachineModule =
    {
      services.arrivin.client = {
        url = "http://server/graphql";
      };
    };
in
testers.runNixOSTest {
  name = "integration-test-empty";
  nodes = {
    server = {
      imports = [
        commonMachineModule
      ];

      services.arrivin.server = {
        enable = true;
      };
    };

    client-publish = {
      imports = [
        commonMachineModule
        clientMachineModule
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

    client-deploy = {
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

  '';
}
