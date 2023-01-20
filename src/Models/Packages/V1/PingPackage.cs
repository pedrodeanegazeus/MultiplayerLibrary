using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Models.Packages.V1;

public class PingPackage : Package, IPackage
{
    public ushort Type => (ushort)PackageType.Ping;
    public byte Version => 1;

    bool IPackage.Compressed => false;

    public Task<byte[]> ToByteArrayAsync()
    {
        return Task.FromResult(Array.Empty<byte>());
    }
}
