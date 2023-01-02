using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class JoinChannelResponsePackage : Package, IPackage
{
    public override PackageType Type => PackageType.JoinChannelResponse;
    public override byte Version => 1;

    bool IPackage.Compressed => false;

    [PackageFieldAttribute(1, FieldType.Guid)]
    public Guid ChannelId { get; }

    [PackageFieldAttribute(2, FieldType.String)]
    public string Channel { get; }

    public JoinChannelResponsePackage(Guid channelId, string channel)
    {
        ChannelId = channelId;
        Channel = channel;
    }
}
