using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class JoinChannelPackage : Package, IPackage
{
    public override PackageType Type => PackageType.JoinChannel;
    public override byte Version => 1;

    bool IPackage.Compressed => false;

    [Order(1)]
    public string Channel { get; }

    public JoinChannelPackage(string channel)
    {
        Channel = channel;
    }
}
