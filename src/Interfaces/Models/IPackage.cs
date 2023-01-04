using MultiplayerLibrary.Models.Packages;

namespace MultiplayerLibrary.Interfaces.Models;

public interface IPackage
{
    PackageType Type { get; }
    byte Version { get; }

    protected internal bool Compressed { get; }

    Task<byte[]> ToByteArrayAsync();
}
