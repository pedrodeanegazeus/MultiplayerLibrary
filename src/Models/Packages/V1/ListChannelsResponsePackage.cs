using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class ListChannelsResponsePackage : Package, IPackage
{
    public override PackageType Type => PackageType.ListChannelsResponse;
    public override byte Version => 1;

    bool IPackage.Compressed => true;

    [PackageFieldAttribute(1, FieldType.Channels)]
    public List<(Guid channelId, string channelName, short connectionsCount)> Channels { get; }

    public ListChannelsResponsePackage(List<(Guid, string, short)> channels)
    {
        Channels = channels;
    }
}
