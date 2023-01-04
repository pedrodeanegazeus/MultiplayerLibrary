using MultiplayerLibrary.Attributes;
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
}
