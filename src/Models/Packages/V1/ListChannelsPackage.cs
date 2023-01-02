using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class ListChannelsPackage : Package, IPackage
{
    public override PackageType Type => PackageType.ListChannels;
    public override byte Version => 1;

    bool IPackage.Compressed => false;
}
