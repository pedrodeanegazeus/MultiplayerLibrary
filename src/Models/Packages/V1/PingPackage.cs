using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class PingPackage : Package, IPackage
{
    public override PackageType Type => PackageType.Ping;
    public override byte Version => 1;

    bool IPackage.Compressed => false;
}
