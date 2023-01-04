using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Extensions;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class LeaveChannelPackage : Package, IPackage
{
    public PackageType Type => PackageType.LeaveChannel;
    public byte Version => 1;

    bool IPackage.Compressed => false;

    [PackageFieldAttribute(1, FieldType.String)]
    public string ChannelName { get; }

    public LeaveChannelPackage(string channelName)
    {
        ChannelName = channelName;
    }

    public async Task<byte[]> ToByteArrayAsync()
    {
        using MemoryStream packageStream = new();
        // ChannelName
        byte[] channelNameBytes = ChannelName.ToUTF8ByteArray();
        await packageStream.WriteAsync(((short)channelNameBytes.Length).ToByteArray());
        await packageStream.WriteAsync(channelNameBytes);
        return packageStream.ToArray();
    }
}
