using System.IO.Compression;
using MultiplayerLibrary.Interfaces.Services;

namespace MultiplayerLibrary.Services;

internal class CompressionService : ICompressionService
{
    public async Task<byte[]> GunzipPackageAsync(byte[] gzippedBytes)
    {
        using MemoryStream inputMemoryStream = new(gzippedBytes);
        using MemoryStream outputMemoryStream = new();
        using GZipStream gzipStream = new(inputMemoryStream, CompressionMode.Decompress);
        await gzipStream.CopyToAsync(outputMemoryStream);
        await gzipStream.FlushAsync();
        byte[] bytes = outputMemoryStream.ToArray();
        return bytes;
    }

    public async Task<byte[]> GzipPackageAsync(byte[] bytes)
    {
        using MemoryStream inputMemoryStream = new(bytes);
        using MemoryStream outputMemoryStream = new();
        using GZipStream gzipStream = new(outputMemoryStream, CompressionLevel.SmallestSize);
        await inputMemoryStream.CopyToAsync(gzipStream);
        await gzipStream.FlushAsync();
        byte[] gzippedBytes = outputMemoryStream.ToArray();
        return gzippedBytes;
    }
}
