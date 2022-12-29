using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class MessagePackage : Package, IPackage
{
    public override PackageType Type => PackageType.Message;
    public override byte Version => 1;

    bool IPackage.Compressed => true;

    [Order(1)]
    public Guid From { get; }

    [Order(2)]
    public Guid To { get; }

    [Order(3)]
    public string Message { get; }

    public MessagePackage(Guid from, Guid to, string message)
    {
        From = from;
        To = to;
        Message = message;
    }
}
