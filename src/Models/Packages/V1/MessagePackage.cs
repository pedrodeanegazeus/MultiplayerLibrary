using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class MessagePackage : Package, IPackage
{
    public override PackageType Type => PackageType.Message;
    public override byte Version => 1;

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
}
