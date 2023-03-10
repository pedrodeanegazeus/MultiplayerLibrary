using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Extensions;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class ErrorPackage : Package, IPackage
{
    public ushort Type => (ushort)PackageType.Error;
    public byte Version => 1;

    bool IPackage.Compressed => false;

    [PackageField(1, FieldType.Short)]
    public short Code { get; }

    [PackageField(2, FieldType.String)]
    public string Message { get; }

    public ErrorPackage(short code, string message)
    {
        Code = code;
        Message = message;
    }

    public async Task<byte[]> ToByteArrayAsync()
    {
        using MemoryStream packageStream = new();
        // Code
        byte[] codeBytes = Code.ToByteArray();
        await packageStream.WriteAsync(codeBytes);
        // Message
        byte[] messageBytes = Message.ToUTF8ByteArray();
        await packageStream.WriteAsync(((short)messageBytes.Length).ToByteArray());
        await packageStream.WriteAsync(messageBytes);
        return packageStream.ToArray();
    }
}
