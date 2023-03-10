using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class HandshakePackage : Package, IPackage
{
    public ushort Type => (ushort)PackageType.Handshake;
    public byte Version => 1;

    bool IPackage.Compressed => false;

    [PackageField(1, FieldType.Guid)]
    public Guid To { get; }

    public HandshakePackage(Guid to)
    {
        To = to;
    }

    public async Task<byte[]> ToByteArrayAsync()
    {
        using MemoryStream packageStream = new();
        // To
        byte[] toBytes = To.ToByteArray();
        await packageStream.WriteAsync(toBytes);
        return packageStream.ToArray();
    }
}
