namespace MultiplayerLibrary.Interfaces.Services;

public interface ICompressionService
{
    Task<byte[]> GunzipPackageAsync(byte[] gzippedBytes);
    Task<byte[]> GzipPackageAsync(byte[] bytes);
}
