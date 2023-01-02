using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class LeaveChannelPackage : Package, IPackage
{
    public override PackageType Type => PackageType.LeaveChannel;
    public override byte Version => 1;

    bool IPackage.Compressed => false;

    [PackageFieldAttribute(1, FieldType.String)]
    public string Channel { get; }

    public LeaveChannelPackage(string channel)
    {
        Channel = channel;
    }
}
