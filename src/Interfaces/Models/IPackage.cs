namespace MultiplayerLibrary.Interfaces.Models;

public interface IPackage
{
    protected internal bool Compressed { get; }

    Task<byte[]> ToByteArrayAsync();
}
