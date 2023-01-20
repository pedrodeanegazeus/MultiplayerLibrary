using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Extensions;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class ListChannelPackage : Package, IPackage
{
    public ushort Type => (ushort)PackageType.ListChannel;
    public byte Version => 1;

    bool IPackage.Compressed => false;

    [PackageField(1, FieldType.String)]
    public string ChannelName { get; }

    public ListChannelPackage(string channelName)
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
