using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Extensions;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class AuthenticatePackage : Package, IPackage
{
    public override PackageType Type => PackageType.Authenticate;
    public override byte Version => 1;

    bool IPackage.Compressed => false;

    [PackageFieldAttribute(1, FieldType.String)]
    public string Token { get; }

    public AuthenticatePackage(string token)
    {
        Token = token;
    }

    public async Task<byte[]> ToByteArrayAsync()
    {
        using MemoryStream packageStream = new();
        // Token
        byte[] tokenBytes = Token.ToUTF8ByteArray();
        await packageStream.WriteAsync(((short)tokenBytes.Length).ToByteArray());
        await packageStream.WriteAsync(tokenBytes);
        return packageStream.ToArray();
    }
}
