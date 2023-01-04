using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Extensions;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class JoinChannelResponsePackage : Package, IPackage
{
    public PackageType Type => PackageType.JoinChannelResponse;
    public byte Version => 1;

    bool IPackage.Compressed => false;

    [PackageFieldAttribute(1, FieldType.Guid)]
    public Guid ChannelId { get; }

    [PackageFieldAttribute(2, FieldType.String)]
    public string ChannelName { get; }

    public JoinChannelResponsePackage(Guid channelId, string channelName)
    {
        ChannelId = channelId;
        ChannelName = channelName;
    }

    public async Task<byte[]> ToByteArrayAsync()
    {
        using MemoryStream packageStream = new();
        // ChannelId
        byte[] channelIdBytes = ChannelId.ToByteArray();
        await packageStream.WriteAsync(channelIdBytes);
        // ChannelName
        byte[] channelNameBytes = ChannelName.ToUTF8ByteArray();
        await packageStream.WriteAsync(((short)channelNameBytes.Length).ToByteArray());
        await packageStream.WriteAsync(channelNameBytes);
        return packageStream.ToArray();
    }
}
