using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class ErrorPackage : Package, IPackage
{
    public override PackageType Type => PackageType.Error;
    public override byte Version => 1;

    bool IPackage.Compressed => false;

    [PackageFieldAttribute(1, FieldType.Short)]
    public short Code { get; }

    [PackageFieldAttribute(2, FieldType.String)]
    public string Message { get; }

    public ErrorPackage(short code, string message)
    {
        Code = code;
        Message = message;
    }
}
