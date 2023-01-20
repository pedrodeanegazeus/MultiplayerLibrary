using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Extensions;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class ListChannelResponsePackage : Package, IPackage
{
    public ushort Type => (ushort)PackageType.ListChannelResponse;
    public byte Version => 1;

    bool IPackage.Compressed => true;

    [PackageField(1, FieldType.Guid)]
    public Guid ChannelId { get; }

    [PackageField(2, FieldType.String)]
    public string ChannelName { get; }

    [PackageField(3, FieldType.PlayerInfoArray)]
    public (Guid connection, string avatarUrl, string displayName)[] PlayersInfo { get; }

    public ListChannelResponsePackage(Guid channelId, string channelName, (Guid, string, string)[] playersInfo)
    {
        ChannelId = channelId;
        ChannelName = channelName;
        PlayersInfo = playersInfo;
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
        await packageStream.WriteAsync(((short)PlayersInfo.Length).ToByteArray());
        foreach ((Guid connection, string avatarUrl, string displayName) in PlayersInfo)
        {
            // Connection
            byte[] connectionValueBytes = connection.ToByteArray();
            await packageStream.WriteAsync(connectionValueBytes);
            // AvatarUrl
            byte[] avatarUrlValueBytes = avatarUrl.ToUTF8ByteArray();
            await packageStream.WriteAsync(((short)avatarUrlValueBytes.Length).ToByteArray());
            await packageStream.WriteAsync(avatarUrlValueBytes);
            // DisplayName
            byte[] displayNameValueBytes = displayName.ToUTF8ByteArray();
            await packageStream.WriteAsync(((short)displayNameValueBytes.Length).ToByteArray());
            await packageStream.WriteAsync(displayNameValueBytes);
        }
        return packageStream.ToArray();
    }
}
