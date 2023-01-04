using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Extensions;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class MessagePackage : Package, IPackage
{
    public PackageType Type => PackageType.Message;
    public byte Version => 1;

    bool IPackage.Compressed => true;

    [PackageFieldAttribute(1, FieldType.Guid)]
    public Guid From { get; }

    [PackageFieldAttribute(2, FieldType.Guid)]
    public Guid To { get; }

    [PackageFieldAttribute(3, FieldType.String)]
    public string Message { get; }

    public MessagePackage(Guid from, Guid to, string message)
    {
        From = from;
        To = to;
        Message = message;
    }

    public async Task<byte[]> ToByteArrayAsync()
    {
        using MemoryStream packageStream = new();
        // From
        byte[] fromBytes = From.ToByteArray();
        await packageStream.WriteAsync(fromBytes);
        // To
        byte[] toBytes = To.ToByteArray();
        await packageStream.WriteAsync(toBytes);
        // Message
        byte[] messageBytes = Message.ToUTF8ByteArray();
        await packageStream.WriteAsync(((short)messageBytes.Length).ToByteArray());
        await packageStream.WriteAsync(messageBytes);
        return packageStream.ToArray();
    }
}
