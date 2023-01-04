using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Extensions;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class ListChannelResponsePackage : Package, IPackage
{
    public override PackageType Type => PackageType.ListChannelResponse;
    public override byte Version => 1;

    bool IPackage.Compressed => true;

    [PackageFieldAttribute(1, FieldType.Guid)]
    public Guid ChannelId { get; }

    [PackageFieldAttribute(2, FieldType.String)]
    public string ChannelName { get; }

    [PackageFieldAttribute(3, FieldType.GuidArray)]
    public Guid[] Connections { get; }

    public ListChannelResponsePackage(Guid channelId, string channelName, Guid[] connections)
    {
        ChannelId = channelId;
        ChannelName = channelName;
        Connections = connections;
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
        // Connections
        await packageStream.WriteAsync(((short)Connections.Length).ToByteArray());
        foreach (Guid connection in Connections)
        {
            byte[] connectionValueBytes = connection.ToByteArray();
            await packageStream.WriteAsync(connectionValueBytes);
        }
        return packageStream.ToArray();
    }
}
