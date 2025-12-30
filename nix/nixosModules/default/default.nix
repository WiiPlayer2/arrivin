{ outputs, ... }:
{
  imports = [
    outputs.nixosModules.arrivin
  ];

  nixpkgs.overlays = [
    outputs.overlays.default
  ];
}
