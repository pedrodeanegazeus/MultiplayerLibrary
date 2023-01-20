using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Extensions;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class AuthenticateResponsePackage : Package, IPackage
{
    public ushort Type => (ushort)PackageType.AuthenticateResponse;
    public byte Version => 1;

    bool IPackage.Compressed => true;

    [PackageField(1, FieldType.String)]
    public string AvatarUrl { get; }

    [PackageField(2, FieldType.String)]
    public string DisplayName { get; }

    public AuthenticateResponsePackage(string avatarUrl, string displayName)
    {
        AvatarUrl = avatarUrl;
        DisplayName = displayName;
    }

    public async Task<byte[]> ToByteArrayAsync()
    {
        using MemoryStream packageStream = new();
        // AvatarUrl
        byte[] avatarUrlBytes = AvatarUrl.ToUTF8ByteArray();
        await packageStream.WriteAsync(((short)avatarUrlBytes.Length).ToByteArray());
        await packageStream.WriteAsync(avatarUrlBytes);
        // DisplayName
        byte[] displayNameBytes = DisplayName.ToUTF8ByteArray();
        await packageStream.WriteAsync(((short)displayNameBytes.Length).ToByteArray());
        await packageStream.WriteAsync(displayNameBytes);
        return packageStream.ToArray();
    }
}
