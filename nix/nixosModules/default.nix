{ outputs, ... } @ args:
{
  arrivin = import ./arrivin args;
  default = import ./default args;
}
