using MultiplayerLibrary.Attributes;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class HandshakePackage : Package, IPackage
{
    public override PackageType Type => PackageType.Handshake;
    public override byte Version => 1;

    bool IPackage.Compressed => false;

    [Order(1)]
    public Guid To { get; }

    public HandshakePackage(Guid to)
    {
        To = to;
    }
}
